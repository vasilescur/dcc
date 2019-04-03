.text
    # Set up the stack pointer 
    la      $r1,    __sp_init
    lw      $r6,    0($r1)
    xor     $r1,    $r1,    $r1

    # Jump to the entry point
    jal     main
    halt

# putc (Int c)
putc:
    # Allocate stack space and back up registers
    addi    $r6,    $r6,    -6
    sw      $r1,    0($r6)
    sw      $r2,    1($r6)
    sw      $r3,    2($r6)
    sw      $r4,    3($r6)
    xor     $r5,    $r5,    $r5
    sw      $r7,    4($r6)
    sw      $r0,    5($r6)

    # [InlineAssembly]
    lw      $r1,    0($r6)
    output  $r1
    

    # [Return ]
    lw      $r1,    0($r6)
    lw      $r2,    1($r6)
    lw      $r3,    2($r6)
    lw      $r4,    3($r6)
    lw      $r7,    4($r6)
    addi    $r6,    $r6,    6
    jr      $r7

# getc ()
getc:
    # Allocate stack space and back up registers
    addi    $r6,    $r6,    -5
    sw      $r1,    0($r6)
    sw      $r2,    1($r6)
    sw      $r3,    2($r6)
    sw      $r4,    3($r6)
    xor     $r5,    $r5,    $r5
    sw      $r7,    4($r6)

    # [InlineAssembly]
    input   $r5
    

    # [Return ]
    lw      $r1,    0($r6)
    lw      $r2,    1($r6)
    lw      $r3,    2($r6)
    lw      $r4,    3($r6)
    lw      $r7,    4($r6)
    addi    $r6,    $r6,    5
    jr      $r7

# main ()
main:
    # Allocate stack space and back up registers
    addi    $r6,    $r6,    -5
    sw      $r1,    0($r6)
    sw      $r2,    1($r6)
    sw      $r3,    2($r6)
    sw      $r4,    3($r6)
    xor     $r5,    $r5,    $r5
    sw      $r7,    4($r6)

    # [FunctionCall [Function putc --> Void] [Operation Addition [Literal 48] [Operation Addition [Literal 3] [Operation Subtraction [Literal 1] [Literal 2]]]],
    addi    $r6,    $r6,    -1
    # [EvaluateExpression [Operation Addition [Literal 48] [Operation Addition [Literal 3] [Operation Subtraction [Literal 1] [Literal 2]]]]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Literal 48]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    31
    addi    $r1,    $r1,    17
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [EvaluateExpression [Operation Addition [Literal 3] [Operation Subtraction [Literal 1] [Literal 2]]]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Literal 3]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    3
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [EvaluateExpression [Operation Subtraction [Literal 1] [Literal 2]]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Literal 1]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    1
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [EvaluateExpression [Literal 2]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    2
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [Operation Subtraction [Literal 1] [Literal 2]]
    lw      $r2,    0($r6)
    lw      $r3,    1($r6)
    sub     $r1,    $r2,    $r3
    addi    $r6,    $r6,    2
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [Operation Addition [Literal 3] [Operation Subtraction [Literal 1] [Literal 2]]]
    lw      $r2,    0($r6)
    lw      $r3,    1($r6)
    add     $r1,    $r2,    $r3
    addi    $r6,    $r6,    2
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [Operation Addition [Literal 48] [Operation Addition [Literal 3] [Operation Subtraction [Literal 1] [Literal 2]]]]
    lw      $r2,    0($r6)
    lw      $r3,    1($r6)
    add     $r1,    $r2,    $r3
    addi    $r6,    $r6,    2
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    jal     putc

    # [FunctionCall [Function putc --> Void] [Literal 10],
    addi    $r6,    $r6,    -1
    # [EvaluateExpression [Literal 10]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    10
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    jal     putc

    # [If [Operation TestEq [Literal 1] [Literal 1]]]
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Operation TestEq [Literal 1] [Literal 1]]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Literal 1]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    1
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [EvaluateExpression [Literal 1]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    1
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [Operation TestEq [Literal 1] [Literal 1]]
    lw      $r2,    0($r6)
    lw      $r3,    1($r6)
    addi    $r1,    $r0,    1
    beq     $r2,    $r3,    __beq_7
    addi    $r1,    $r0,    0
  __beq_7:
    addi    $r6,    $r6,    2
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    lw      $r1,    1($r6)
    addi    $r6,    $r6,    1
    beq     $r1,    $r0,    __if_skip_8

    # [FunctionCall [Function putc --> Void] [Literal 89],
    addi    $r6,    $r6,    -1
    # [EvaluateExpression [Literal 89]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    31
    addi    $r1,    $r1,    31
    addi    $r1,    $r1,    27
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    jal     putc

    # [FunctionCall [Function putc --> Void] [Literal 10],
    addi    $r6,    $r6,    -1
    # [EvaluateExpression [Literal 10]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    10
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    jal     putc
  __if_skip_8:
    addi    $r6,    $r6,    1

    # [If [Operation TestEq [Literal 1] [Literal 2]]]
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Operation TestEq [Literal 1] [Literal 2]]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Literal 1]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    1
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [EvaluateExpression [Literal 2]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    2
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [Operation TestEq [Literal 1] [Literal 2]]
    lw      $r2,    0($r6)
    lw      $r3,    1($r6)
    addi    $r1,    $r0,    1
    beq     $r2,    $r3,    __beq_13
    addi    $r1,    $r0,    0
  __beq_13:
    addi    $r6,    $r6,    2
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    lw      $r1,    1($r6)
    addi    $r6,    $r6,    1
    beq     $r1,    $r0,    __if_skip_14

    # [FunctionCall [Function putc --> Void] [Literal 78],
    addi    $r6,    $r6,    -1
    # [EvaluateExpression [Literal 78]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    31
    addi    $r1,    $r1,    31
    addi    $r1,    $r1,    16
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    jal     putc

    # [FunctionCall [Function putc --> Void] [Literal 10],
    addi    $r6,    $r6,    -1
    # [EvaluateExpression [Literal 10]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    10
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    jal     putc
  __if_skip_14:
    addi    $r6,    $r6,    1

    # [FunctionCall [Function putc --> Void] [Operation Addition [Literal 48] [Operation Subtraction [Operation Addition [Literal 1] [Literal 6]] [Operation Addition [Literal 2] [Operation Subtraction [Operation Addition [Literal 4] [Literal 1]] [Literal 3]]]]],
    addi    $r6,    $r6,    -1
    # [EvaluateExpression [Operation Addition [Literal 48] [Operation Subtraction [Operation Addition [Literal 1] [Literal 6]] [Operation Addition [Literal 2] [Operation Subtraction [Operation Addition [Literal 4] [Literal 1]] [Literal 3]]]]]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Literal 48]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    31
    addi    $r1,    $r1,    17
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [EvaluateExpression [Operation Subtraction [Operation Addition [Literal 1] [Literal 6]] [Operation Addition [Literal 2] [Operation Subtraction [Operation Addition [Literal 4] [Literal 1]] [Literal 3]]]]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Operation Addition [Literal 1] [Literal 6]]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Literal 1]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    1
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [EvaluateExpression [Literal 6]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    6
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [Operation Addition [Literal 1] [Literal 6]]
    lw      $r2,    0($r6)
    lw      $r3,    1($r6)
    add     $r1,    $r2,    $r3
    addi    $r6,    $r6,    2
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [EvaluateExpression [Operation Addition [Literal 2] [Operation Subtraction [Operation Addition [Literal 4] [Literal 1]] [Literal 3]]]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Literal 2]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    2
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [EvaluateExpression [Operation Subtraction [Operation Addition [Literal 4] [Literal 1]] [Literal 3]]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Operation Addition [Literal 4] [Literal 1]]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r6,    $r6,    -2
    # [EvaluateExpression [Literal 4]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    4
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [EvaluateExpression [Literal 1]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    1
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [Operation Addition [Literal 4] [Literal 1]]
    lw      $r2,    0($r6)
    lw      $r3,    1($r6)
    add     $r1,    $r2,    $r3
    addi    $r6,    $r6,    2
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [EvaluateExpression [Literal 3]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    3
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [Operation Subtraction [Operation Addition [Literal 4] [Literal 1]] [Literal 3]]
    lw      $r2,    0($r6)
    lw      $r3,    1($r6)
    sub     $r1,    $r2,    $r3
    addi    $r6,    $r6,    2
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [Operation Addition [Literal 2] [Operation Subtraction [Operation Addition [Literal 4] [Literal 1]] [Literal 3]]]
    lw      $r2,    0($r6)
    lw      $r3,    1($r6)
    add     $r1,    $r2,    $r3
    addi    $r6,    $r6,    2
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [Operation Subtraction [Operation Addition [Literal 1] [Literal 6]] [Operation Addition [Literal 2] [Operation Subtraction [Operation Addition [Literal 4] [Literal 1]] [Literal 3]]]]
    lw      $r2,    0($r6)
    lw      $r3,    1($r6)
    sub     $r1,    $r2,    $r3
    addi    $r6,    $r6,    2
    sw      $r1,    2($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    # [Operation Addition [Literal 48] [Operation Subtraction [Operation Addition [Literal 1] [Literal 6]] [Operation Addition [Literal 2] [Operation Subtraction [Operation Addition [Literal 4] [Literal 1]] [Literal 3]]]]]
    lw      $r2,    0($r6)
    lw      $r3,    1($r6)
    add     $r1,    $r2,    $r3
    addi    $r6,    $r6,    2
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    jal     putc

    # [FunctionCall [Function putc --> Void] [Literal 10],
    addi    $r6,    $r6,    -1
    # [EvaluateExpression [Literal 10]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    10
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    jal     putc

    # [Return [Literal 0]]
    addi    $r6,    $r6,    -1
    # [EvaluateExpression [Literal 0]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    0
    sw      $r1,    1($r6)
    lw      $r1,    0($r6)
    addi    $r6,    $r6,    1
    xor     $r5,    $r5,    $r5
    lw      $r5,    0($r6)
    addi    $r6,    $r6,    1
    lw      $r1,    0($r6)
    lw      $r2,    1($r6)
    lw      $r3,    2($r6)
    lw      $r4,    3($r6)
    lw      $r7,    4($r6)
    addi    $r6,    $r6,    5
    jr      $r7

.data
# Initial stack pointer value
__sp_init: .word 32767

# Global Variables: 