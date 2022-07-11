from enum import Enum
import math
from  numpy import array


def test_func():
    test_result = get_rotate_offset_position(RotateAxis.OY, 0, RotateAxis.OX, 5, RotateAxis.OZ, 2,
                                             RotateAxis.OY, 0.4048796, [0, 0.8378, 0])

    print("RESULT ******* ", test_result)


class RotateAxis(Enum):
    OX = 1
    OY = 2
    OZ = 3


def normalize_coordinates(full_real_hand_length: float, full_unity_hand_length: float, old_coord: list) -> list:
    new_x = old_coord[0] * full_unity_hand_length / full_real_hand_length
    new_y = old_coord[1] * full_unity_hand_length / full_real_hand_length
    new_z = old_coord[2] * full_unity_hand_length / full_real_hand_length
    return [new_x, new_y, new_z]


def get_rotate_offset_position(rotate_axis0: RotateAxis, rotate_angle0: float, rotate_axis1: RotateAxis,
                               rotate_angle1: float,
                               rotate_axis2: RotateAxis, rotate_angle2: float,
                               offset_axis: RotateAxis, offset_dist: float, end_position: list) -> list:
    # displacement to the second joint of the arm and rotation around its coordinate system
    radians = angle_to_radians(rotate_angle2)
    new_position1 = rotate_and_offset(offset_axis, offset_dist, rotate_axis2, radians, end_position)
    # rotation about the main coordinate system by an angle to tilt the arm
    radians = angle_to_radians(rotate_angle1)
    new_position2 = rotate_around_axis(new_position1, rotate_axis1, radians)
    # rotation about the main coordinate system by an angle around its own axis
    radians = angle_to_radians(rotate_angle0)
    new_position3 = rotate_around_axis(new_position2, rotate_axis0, radians)

    return new_position3


def angle_to_radians(angle: float) -> float:
    return angle * math.pi / 180


def rotate_and_offset(offset_axis1: RotateAxis, offset1: float, rotate_axis: RotateAxis, alpha: float,
                      old_coord: list) -> list:
    # n, p - vertical, m, q - horizontal
    # n, m - first matrix, p, q - second
    n = 4
    m = 4
    p = 4
    q = 4
    offset_matrix = get_offset_matrix(offset_axis1, -offset1)
    rotate_matrix = get_rotate_matrix(rotate_axis, alpha, 4)
    old_coordinates = [[old_coord[0]], [old_coord[1]], [old_coord[2]], [1]]

    # offset to(0, 0, 0) coordinates system
    new_coords = matrix_multiplication(offset_matrix, old_coordinates, m, n, p, 1)

    # rotate
    result = matrix_multiplication(rotate_matrix, new_coords, m, n, p, 1)

    # offset back
    offset_matrix = get_offset_matrix(offset_axis1, offset1)
    result = matrix_multiplication(offset_matrix, result, m, n, p, 1)

    res_vector = [result[0][0], result[1][0], result[2][0]]
    return res_vector


def get_offset_matrix(offset_axis: RotateAxis, offset: float) -> list:
    offset_matrix = []
    if offset_axis == RotateAxis.OX:
        offset_matrix = [
            [1, 0, 0, offset],
            [0, 1, 0, 0],
            [0, 0, 1, 0],
            [0, 0, 0, 1]]
    if offset_axis == RotateAxis.OY:
        offset_matrix = [
            [1, 0, 0, 0],
            [0, 1, 0, offset],
            [0, 0, 1, 0],
            [0, 0, 0, 1]]
    if offset_axis == RotateAxis.OZ:
        offset_matrix = [
            [1, 0, 0, 0],
            [0, 1, 0, 0],
            [0, 0, 1, offset],
            [0, 0, 0, 1]]
    return offset_matrix


def get_rotate_matrix(rotate_axis: RotateAxis, alpha: float, count_of_elements=3) -> list:
    rotate_matrix = []
    if rotate_axis == RotateAxis.OX:
        if count_of_elements == 3:
            rotate_matrix = [
                [1, 0, 0],
                [0, math.cos(alpha), -math.sin(alpha)],
                [0, math.sin(alpha), math.cos(alpha)]]
        elif count_of_elements == 4:
            rotate_matrix = [
                [1, 0, 0, 0],
                [0, math.cos(alpha), -math.sin(alpha), 0],
                [0, math.sin(alpha), math.cos(alpha)],
                [0, 0, 0, 1]]

    if rotate_axis == RotateAxis.OY:
        if count_of_elements == 3:
            rotate_matrix = [
                [math.cos(alpha), 0, math.sin(alpha)],
                [0, 1, 0],
                [-math.sin(alpha), 0, math.cos(alpha)]]
        elif count_of_elements == 4:
            rotate_matrix = [
                [math.cos(alpha), 0, math.sin(alpha), 0],
                [0, 1, 0, 0],
                [-math.sin(alpha), 0, math.cos(alpha), 0],
                [0, 0, 0, 1]]

    if rotate_axis == RotateAxis.OZ:
        if count_of_elements == 3:
            rotate_matrix = [
                [math.cos(alpha), -math.sin(alpha), 0],
                [math.sin(alpha), math.cos(alpha), 0],
                [0, 0, 1]]
        elif count_of_elements == 4:
            rotate_matrix = [
                [math.cos(alpha), -math.sin(alpha), 0, 0],
                [math.sin(alpha), math.cos(alpha), 0, 0],
                [0, 0, 1, 0],
                [0, 0, 0, 1]]

    return rotate_matrix


def matrix_multiplication(a: list, b: list, m: int, n: int, p: int, q: int) -> list:
    c = []

    for i in range(m):
        c.append([])
        for j in range(q):
            c[i].append(0)

    if n != p:
        return c
    for i in range(m):
        for j in range(q):
            for k in range(n):
                c[i][j] += a[i][k] * b[k][j]
    return c


def rotate_around_axis(old_coords: list, axis: RotateAxis, alpha: float) -> list:
    n = 3
    m = 3
    p = 3
    q = 1
    rotate_matrix = get_rotate_matrix(axis, alpha)
    old_coordinates = [[old_coords[0]], [old_coords[1]], [old_coords[2]]]
    result = matrix_multiplication(rotate_matrix, old_coordinates, m, n, p, q)
    res_vector = [result[0][0], result[1][0], result[2][0]]
    return res_vector
