# Conventions

## Memory Layout

- Instruction Memory
    - Text: `0x0000` – `0xFFFF`

- Data Memory
    - Stack: Stack pointer is initially `0x7FFF`. Grows down
    - Heap: `0x2000` and grows up
    - Static Data: `0x0000` – `0x1FFF`

## Registers

There are 8 registers in the register file of the Duke 250/16, in addition to the `PC`
program counter register (which is not in the register file). These registers are named
`r0` through `r7`. Registers can each hold one word, which is two bytes.

Here are their conventional uses and descriptions:

- `r0` is always equal to `0`
- `r1` – `r4` are general-purpose callee-saved registers
    - May be used to pass parameters
- `r5` is the return value
- `r6` is the stack pointer
- `r7` is the link register for `jal`, similar to `$ra` in MIPS

## Functions and Variables

All local variables will live on the stack. The compiler will keep track of the stack
for the local scope. Whenever an operation needs to manipulate local variables, they will
all be copied from the stack into registers `r1` – `r5` (as needed), the operation will be
performed on them, and then they will be copied back into their corresponding positions on
the stack.