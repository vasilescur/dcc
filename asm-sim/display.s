.text
# This program is a test of the display driver.

        la      $r4,    videorange
        lw      $r3,    0($r4)

        addi    $r5,    $r0,    5
        addi    $r1,    $r0,    1

loop:   sw      $r1,    3($r3)
        shl     $r1,    $r1,    1
        addi    $r2,    $r0,    1

        blt     $r2,    $r5,    loop

        halt

.data

videorange: .word 45056         # 0xb000
