#!/usr/bin/env python3
"""Generates src/WinCapture.App/Assets/AppIcon.ico from scratch (original artwork, no external images).

Design: accent blue (#2F7CF6) rounded square, four white corner brackets (a viewfinder) and a white center dot.
Same family as the tray icon (blue rounded square + white dot). Pure Python standard library only.
Usage: python3 scripts/generate-app-icon.py
"""
import os
import struct
import zlib

SIZES = [16, 24, 32, 48, 64, 128, 256]
BLUE = (0x2F, 0x7C, 0xF6)
WHITE = (255, 255, 255)
SS = 4  # supersampling factor for anti-aliasing


def coverage(px, py):
    """Returns (in_square, in_white) for a point in the 0..256 design space."""
    # Rounded square 0..256, corner radius 58.
    r = 58.0
    cx = min(max(px, r), 256 - r)
    cy = min(max(py, r), 256 - r)
    in_sq = (px - cx) ** 2 + (py - cy) ** 2 <= r * r

    white = False
    # Corner brackets: margin 62, arm length 56, thickness 20.
    m, ln, t = 62.0, 56.0, 20.0
    lo, hi = m, 256 - m
    for (x0, y0, sx, sy) in ((lo, lo, 1, 1), (hi, lo, -1, 1), (lo, hi, 1, -1), (hi, hi, -1, -1)):
        # Horizontal arm and vertical arm of the L, anchored at the bracket corner (x0, y0).
        hx = (x0, x0 + sx * ln) if sx > 0 else (x0 + sx * ln, x0)
        hy = (y0, y0 + sy * t) if sy > 0 else (y0 + sy * t, y0)
        vx = (x0, x0 + sx * t) if sx > 0 else (x0 + sx * t, x0)
        vy = (y0, y0 + sy * ln) if sy > 0 else (y0 + sy * ln, y0)
        if hx[0] <= px <= hx[1] and hy[0] <= py <= hy[1]:
            white = True
        if vx[0] <= px <= vx[1] and vy[0] <= py <= vy[1]:
            white = True
    # Center dot radius 22.
    if (px - 128) ** 2 + (py - 128) ** 2 <= 22 * 22:
        white = True
    return in_sq, white


def render(size):
    rows = []
    n = SS * SS
    for y in range(size):
        row = bytearray()
        for x in range(size):
            sq = wh = 0
            for sy in range(SS):
                for sx in range(SS):
                    px = (x + (sx + 0.5) / SS) * 256.0 / size
                    py = (y + (sy + 0.5) / SS) * 256.0 / size
                    a, b = coverage(px, py)
                    if a:
                        sq += 1
                        if b:
                            wh += 1
            if sq == 0:
                row += bytes((0, 0, 0, 0))
                continue
            t = wh / sq  # share of white inside the square
            col = tuple(round(BLUE[i] * (1 - t) + WHITE[i] * t) for i in range(3))
            row += bytes((col[0], col[1], col[2], round(255 * sq / n)))
        rows.append(bytes(row))
    return rows


def png(size, rows):
    def chunk(tag, data):
        c = struct.pack(">I", len(data)) + tag + data
        return c + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)

    raw = b"".join(b"\x00" + r for r in rows)
    return (b"\x89PNG\r\n\x1a\n"
            + chunk(b"IHDR", struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0))
            + chunk(b"IDAT", zlib.compress(raw, 9))
            + chunk(b"IEND", b""))


def main():
    images = [(s, png(s, render(s))) for s in SIZES]
    header = struct.pack("<HHH", 0, 1, len(images))
    offset = 6 + 16 * len(images)
    entries, blobs = b"", b""
    for s, data in images:
        entries += struct.pack("<BBBBHHII", 0 if s >= 256 else s, 0 if s >= 256 else s, 0, 0, 1, 32, len(data), offset)
        blobs += data
        offset += len(data)
    out = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "src", "WinCapture.App", "Assets", "AppIcon.ico")
    with open(out, "wb") as f:
        f.write(header + entries + blobs)
    print("wrote", os.path.normpath(out), "sizes", SIZES)


if __name__ == "__main__":
    main()
