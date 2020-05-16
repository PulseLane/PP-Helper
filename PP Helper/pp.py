l2 = [
    (0,0),
    (40,8),
    (50, 15),
    (69, 25),
    (75, 42.5),
    (82, 56),
    (84.5, 63),
    (86, 72),
    (88, 76.6),
    (90, 81.5),
    (91, 85),
    (92, 88.5),
    (93, 92),
    (94, 97.5),
    (95, 103.6),
    (100, 110),
    (110, 115),
    (114, 120)
]

l = [
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

def lerp(x1, y1, x2, y2, x3):
    m = (y2 - y1) / (x2 - x1)
    return m*(x3 - x1) + y1

def pp_percentage(percentage):
    i = -1
    for score, given in l:
        if score > percentage:
            break
        i+=1
    # max value, just return 120%
    if  i == len(l) - 1:
        l[i][1]
    
    given_percentage = lerp(l[i][0], l[i][1], l[i+1][0], l[i+1][1], percentage)

    print(str(percentage) + "% gives " + str(round(given_percentage *  float(100), 2)) + "% of the pp")
    return given_percentage

def calculate_raw_pp(pp, percentage):
    # raw_pp * percentage_given = pp
    return pp / pp_percentage(percentage)

def calculate_pp(percentage, raw_pp):
    return round(pp_percentage(percentage) * raw_pp, 2)


# print(lerp(1, 1, 2, 3, 1.5))

pp = 378.82
percentage = 95.91
my_percentage = 88.5

raw_pp = calculate_raw_pp(pp, percentage)
print("Raw pp: " + str(round(raw_pp, 2)))
print(calculate_pp(my_percentage, raw_pp))