import json
from urllib.request import urlopen, Request
import lxml
from bs4 import BeautifulSoup
import os
from time import sleep

# too lazy to split into modules, so just gonna use one script for everything :)
DEBUG = True
STATUS_UPDATES = True
PAGE_LIMIT = 1000
FILE_NAME = "raw_pp.json"
# time in seconds
WAIT_BETWEEN_LEADERBOARD_CALLS = 10
WAIT_BETWEEN_API_CALLS = 10
BACKOFF_TIME = 60 * 10

raw_pp_data = {}
songs_calculated = 0

pp_curve = [
    (0,0),
    (45, .015),
    (50, .03),
    (55, .06),
    (60, .105),
    (65, .16),
    (68, .24),
    (70, .285),
    (80, .563),
    (84, .695),
    (88, .826),
    (94.5, 1.015),
    (95, 1.046),
    (100, 1.12),
    (110, 1.18),
    (114, 1.25),
]

def open_url(url):
    done = False
    while not done:
        try:
            response = urlopen(Request(url, headers={"User-Agent": "Raw PP Calculator by PulseLane"}))
            done = True
        # back off, sleep for 10 minutes
        except Exception:
            if DEBUG:
                print("Ran into error opening url: " + url + ", sleeping for 10 minutes")
            sleep(BACKOFF_TIME)
    return response

# strip extraneous data from scoresaber difficulty
def get_diff(difficulty):
    return difficulty[1:difficulty.rfind("_")]

# linearly interpolate to find y3 value for x3 between (x1, y1) and (x2, y2)
def lerp(x1, y1, x2, y2, x3):
    m = (y2 - y1) / (x2 - x1)
    return m*(x3 - x1) + y1

# given a score percentage and its corresponding pp, calculate the raw pp for the map
def calculate_raw_pp(pp, percentage):
    # raw_pp * percentage_given = pp
    return pp / pp_percentage(percentage)

# calculate how much percentage of the raw pp a given percentage is worth
def pp_percentage(percentage):
    i = -1
    for score, given in pp_curve:
        if score > percentage:
            break
        i+=1
    # max value, just return the cap
    if  i == len(pp_curve) - 1:
        return pp_curve[i][1]
    
    given_percentage = lerp(pp_curve[i][0], pp_curve[i][1], pp_curve[i+1][0], pp_curve[i+1][1], percentage)
    # if DEBUG:
    #     print(str(percentage) + "% gives " + str(round(given_percentage *  float(100), 2)) + "% of the pp")

    return given_percentage

# calculates how much pp a score is worth given the raw pp of the map
def calculate_pp(raw_pp, percentage):
    return round(pp_percentage(percentage) * raw_pp, 2)

# calculate the raw pp from the given leaderboard
# should probably implement some error here..
def find_raw_pp(html):
    soup = BeautifulSoup(html, "lxml")

    percentages = soup.findAll("td", {"class": "percentage"})
    percentage = float(percentages[0].text.strip()[:-1])

    pps = soup.findAll("span", {"class": "scoreTop ppValue"})
    pp = float(pps[0].text.strip())

    return calculate_raw_pp(pp, percentage)

# write the json data to the output file
def write_data(data, file):
    with open(file, "w") as output:
        json.dump(data, output)


def get_song_data(song):
    global songs_calculated

    song_data = {}
    uid = song["uid"]
    # I think at least one one-handed map is ranked, so don't strip off fluff from here
    diff = song["diff"]
    
    # get raw pp from leaderboard
    url = "https://scoresaber.com/leaderboard/" + str(uid)
    # this breaks randomly sometimes /shrug
    try:
        # round to 2 decimal places - this is an approximation anyway
        pp = round(find_raw_pp(open_url(url).read()), 2)
        song_data[diff] = pp
    except Exception:
        print("ERROR: Couldn't get raw pp for " +  song["name"] + " - " + str(song["id"]) + ": " + url)
        return song_data

    songs_calculated+=1
    if STATUS_UPDATES or DEBUG:
        print("Song #" + str(songs_calculated) + ": " + song["name"] + ", " + get_diff(diff) + " is worth " + str(pp) + "pp!")

    return song_data

# get the data for all the songs returned in the api call
def get_data(response):
    global raw_pp_data
    done = False
    data = {}
    api_data = json.load(response)
    # stop once every song has been gathered or song we already scraped data for
    for song in api_data["songs"]:
        # skipped unranked songs
        if song["ranked"] != 1:
            continue
        if song["id"] in raw_pp_data and song["diff"] in raw_pp_data[song["id"]]:
            done = True
            break
        # add diff data to dict
        if song["id"] in data:
            data[song["id"]].update(get_song_data(song))
        else:
            data[song["id"]] = get_song_data(song)
        # back off for a bit
        sleep(WAIT_BETWEEN_LEADERBOARD_CALLS)
    if len(api_data["songs"]) == 0:
        done = True
    return (data, done)

# I hope no songs get ranked during this first scraping lol
# main scraping method
def main():
    data = {}
    # go until we reach the first unranked song or first song we have data for
    done = False
    page = 1
    while not done:
        url = "http://scoresaber.com/api.php?function=get-leaderboards&cat=1&page=" + str(page) + "&limit=" + str(PAGE_LIMIT)
        response = open_url(url)
        new_data, done = get_data(response)
        # add the new data to the dict
        for song_id in new_data:
            if song_id in data:
                data[song_id].update(new_data[song_id])
            else:
                data[song_id] = new_data[song_id]
        # back off for a bit
        sleep(WAIT_BETWEEN_API_CALLS)
        page+=1
    return data

# need to think of a way to rescan data for when maps get re-adjusted
if __name__ == "__main__":
    # if file exists, load the data
    if os.path.exists(FILE_NAME):
        if DEBUG:
            print("file exists, some data has already been scraped")
        raw_pp_data = json.load(open(FILE_NAME, "r"))
    raw_pp_data.update(main())
    write_data(raw_pp_data, FILE_NAME)