<p align="center">
    <img src="logo.png" height="250px">
    <br/>
    This is a C compiler targeting the Duke 250/16 architecture.
</p>

<br/>

This project is still very much a work in progress. The end goal is to be able to compile a large enough subset of the C
language to be sufficient for general-purpose demonstrations and implementation of simple algorithms. If I can eventually get 
`HoopsStats.c` compiled and running on this, I'll be a happy man.

**Completion Status**: `80%`. As of now, it can compile full programs and call functions and produce I/O, as well as support for `if` statements, local variables, addition/subtraction, inline ASM. Currently working on debugging some local var stack management stuff.

## Instructions

### Usage

To compile a source file `program.c`, run:

```bash
dcc program.c
```

If using the development version, instead use:

```bash
dotnet run program.c
```

This will produce the output file `program.s`, containing assembly code in the
Duke 250/16 architecture.

#### Duke 250/16 assembler and simulator

There is an assembler and simulator for the Duke 250/16 packaged with this
repository, included in the `asm-sim` folder. These are not written by me. For more information about these 
programs, please contact Professor Daniel Sorin at Duke University.

To assemble and run the compiled program, first build the assembler and simulator from source:

```bash
cd asm-sim
make
```

Then, run the assembler:

```bash
./asm program.s
```

This will produce two output files:

- `program.imem.lgsim` - Instruction memory image
- `program.dmem.lgsm` - Data memory image

These are [Logisim](http://www.cburch.com/logisim/)-compatible memory image files,
which can either be loaded into the RAM and ROM components of a Logisim implementation
of the Duke 250/16, or run with the included simulator.

To run these programs with the included simulator, use:

```bash
./sim program.imem.lgsim program.dmem.lgsim
```

For more details about the Duke 250/16 assembler and simulator, please see the included
`README.md` in the `asm-sim` folder.

## Contributing

Feel free to contribute via pull requests or suggestions. More example programs are also
welcome.

### Build Instructions

This compiler is written in C# .NET. To build, you'll need to install the .NET developer
tools. To check if you have this installed, you can run:

```bash
dotnet --version
```

To build, run:

```bash
dotnet build
```

## The Duke 250/16 Architecture

The Duke 250/16 is a 16-bit MIPS-like, word-addressed RISC architecture created for educational purposes in the `COMPSCI 250` course at Duke University.

### Instruction Set

| Instruction  	| Opcode 	| Type 	| Usage                  	| Operation
|---------  	|--------	|------	|------------------------	|------------
| `lw`         	| `000`    	| `I`  	| `lw $rt, Imm($rs)`       	| `$rt = Mem[$rs+Imm]`
| `sw`         	| `001`    	| `I`  	| `sw $rt, Imm($rs)`       	| `Mem[$rs+Imm] = $rt`
| `beq`        	| `010`   	| `I`  	| `beq $rs, $rt, Imm`      	| `if ($rs==$rt) then PC=PC+1+Imm`
| `blt`        	| `011`   	| `I`  	| `blt $rs, $rt, Imm`      	| `if ($rs<$rt) then PC=PC+1+Imm`
| `not`        	| `100`    	| `R`  	| `not $rd, $rs`           	| `$rd = NOT $rs`
| `xor`        	| `101`    	| `R`  	| `xor $rd, $rs, $rt`      	| `$rd = $rs XOR $rt`
| `addi`       	| `110`    	| `I`  	| `addi $rt, $rs, Imm`     	| `$rt=$rs+Imm`
| `add`        	| `111`    	| `R`  	| `add $rd, $rs, $rt`      	| `$rd=$rs+$rt`
| `sub`        	| `1000`   	| `R`  	| `sub $rd, $rs, $rt`      	| `$rd=$rs-$rt`
| `shra`       	| `1001`   	| `R`  	| `shra $rd, $rs, <shamt>` 	| `$rd` = `$rs` shifted `<shamt>` to right
| `shl`        	| `1010`   	| `R`  	| `slh $rd, $rs, <shamt>`  	| `$rd` = `$rs` shifted `<shamt>` to left
| `j`          	| `1011`   	| `J`  	| `j L`                    	| `PC = L` (upper 4 bits same)
| `jal`        	| `1100`   	| `J`  	| `jal L`                  	| `$r7=PC+1; PC = L`
| `jr`         	| `1101`   	| `R`  	| `jr $rs`                 	| `PC = $rs`
| `output`     	| `1110`   	| `R`  	| `output $rs`             	| print `$rs` on a TTY display
| `input`      	| `1111`   	| `R`  	| `input $rd`              	| `$rd` = keyboard input

### Memory Layout

- Instruction Memory
    - Text: `0x0000` – `0xFFFF`

- Data Memory
    - Stack: Stack pointer is initially `0x7FFF`. Grows down
    - Heap: `0x2000` and grows up
    - Static Data: `0x0000` – `0x1FFF`

### Registers

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

### Functions and Variables

All local variables will live on the stack. The compiler will keep track of the stack
for the local scope. Whenever an operation needs to manipulate local variables, they will
all be copied from the stack into registers `r1` – `r5` (as needed), the operation will be
performed on them, and then they will be copied back into their corresponding positions on
the stack.

## Roadmap

### Compilation Process

- [x] Basic source code parsing
- [x] Preprocessing
- [x] Tokenization
- [x] Parsing into AST
- [ ] Optimization (long-term, low-priority)
- Code generation
    - [x] Targeting Duke 250/16 *(still need file output)*
    - [ ] Targeting regular MIPS (long-term, low-priority)
    - [ ] Targeting x86 (long-term, low-priority)

### Language Features

- [x] Comments and white space
- [x] Preprocessor `#include`
- [ ] Preprocessor `#define`
- [x] Global (static memory) variables
- [x] Functions
- [x] Locally-scoped (stack) variables
- [x] `Expression` evaluation
    - [x] Commonly used operations and expressions (ex. `+`, `-`, `==`, etc.)
    - [ ] Less commonly used operations (ex. `>>`, `>=`, etc.)
- [ ] Pointers and pointer arithmetic
- [ ] Array syntax
- [x] Input and output library (`putc`, `getc`)
- [ ] Control flow statements and blocks
    - [ ] Labels and `goto`
    - [x] `if` and `else`
    - [ ] `for` and `while` loops
    - [ ] `switches` (long-term, low-priority)
- [ ] `enum`s (low-priority)
- [ ] `struct`s
    - [ ] `typedef`
- [ ] Dynamic memory allocation library (`malloc`, `free`)

### Compiler Features

- [ ] Proper error reporting (line number, etc)
- [x] Preprocessor macro for printing a string (`#pragma OutString Hello, world!` --> `putc('H'); putc('e'); ...`)

## Example

Here's a very simple example program:

### Source Files

#### `io.c`
```c
void putc(int c) {
    asm ("
        lw      $r1,    0($r6);
        output  $r1;
    ");

    return;
}

int getc() {
    asm ("
        input   $r5;
    ");

    return; // Implicit return $r5
}
```

#### `test.c`
```c
#include "samples/io.c"

int main() {
    putc('H');
    putc(10);   // \n

    return 0;
}
```

### Compiled Assembly Output

**NOTE**: The comments are generated by the compiler! I did not modify the output in any way; what you see here
is directly copy-pasted from the output file.

```
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
    addi    $r6,    $r6,    -5
    sw      $r1,    0($r6)
    sw      $r2,    1($r6)
    sw      $r3,    2($r6)
    sw      $r4,    3($r6)
    xor     $r5,    $r5,    $r5
    sw      $r7,    4($r6)

    # [InlineAssembly]
    lw      $r1,    0($r6)
    output  $r1
    

    # [Return ]
    lw      $r1,    0($r6)
    lw      $r2,    1($r6)
    lw      $r3,    2($r6)
    lw      $r4,    3($r6)
    lw      $r7,    4($r6)
    addi    $r6,    $r6,    5
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

    # [FunctionCall [Function putc --> Void] [Literal 72],
    addi    $r6,    $r6,    -1
    # [EvaluateExpression [Literal 72]]
    addi    $r6,    $r6,    -1
    sw      $r1,    0($r6)
    xor     $r1,    $r1,    $r1
    addi    $r1,    $r1,    31
    addi    $r1,    $r1,    31
    addi    $r1,    $r1,    10
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
```
