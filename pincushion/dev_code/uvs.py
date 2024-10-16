import math

phi = 1.618033988749894848204586834365638118
q = [
        [-1., phi, 0.],
        [1., phi, 0.],
        [-1., -phi, 0.],
        [1., -phi, 0.],
        [0., -1., phi],
        [0., 1., phi],
        [0., -1., -phi],
        [0., 1., -phi],
        [phi, 0., -1.],
        [phi, 0., 1.],
        [-phi, 0., -1.],
        [-phi, 0., 1.],
    ]
# Normalize.
for i in range(len(q)):
    for j in range(3):
        q[i][j] = (q[i][j] - -phi) / (phi - -phi)
for vertex in q:
    u = (math.atan2(vertex[2],vertex[0]) / (2.0 * math.pi))
    v = (math.asin(vertex[1]) / math.pi) + 0.5
    print(f'[{u}, {v}],')