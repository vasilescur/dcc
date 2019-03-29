#ifndef _FORMAT_H
#define _FORMAT_H

enum instruction_type {I,IBranch,I1Branch,IBranchComp,I1,IDisp,INotRT,INotRD,J,R,R0,R1RS,RDRS,RDRT,R1RT,R1RD,RSH};
struct instruction_t
{
  unsigned short opcode;
  const char* str;
  instruction_type type;
  instruction_t() { str = 0; }
  instruction_t(unsigned short o, const char* s, instruction_type t) { opcode = o; str = s; type = t; }
};

#define OPCODE_LW     0x0
#define OPCODE_SW     0x1
#define OPCODE_BEQ    0x2
#define OPCODE_BLT    0x3
#define OPCODE_NOT    0x4
#define OPCODE_XOR    0x5
#define OPCODE_ADDI   0x6
#define OPCODE_ADD    0x7
#define OPCODE_SUB    0x8
#define OPCODE_SHRA   0x9
#define OPCODE_SHL    0xA
#define OPCODE_J      0xB
#define OPCODE_JAL    0xC
#define OPCODE_JR     0xD
#define OPCODE_OUTPUT 0xE
#define OPCODE_INPUT  0xF

/////////////////////////////////////////////////////////////////////////////////
// unsupported opcodes preserved so we can adapt this kit year to year
#define OPCODE_OR     0x60
#define OPCODE_NAND   0x61
#define OPCODE_SRL    0x62
#define OPCODE_SLL   0x63
#define OPCODE_BGT    0x64
#define OPCODE_SHR    0x65
#define OPCODE_BEQZ    0x66
//#define OPCODE_LW     0x67
//#define OPCODE_SW     0x68
#define OPCODE_BLE    0x69
#define OPCODE_BNE    0x6A
//#define OPCODE_JAL    0x6B
//#define OPCODE_J      0x6C
//#define OPCODE_JR     0x6D
//#define OPCODE_INPUT  0x6E
//#define OPCODE_OUTPUT 0x6F

#define OPCODE_AND 0x70
//#define OPCODE_XOR 0x71
#define OPCODE_ROL 0x72
#define OPCODE_ROR 0x73
//#define OPCODE_BNE 0x74
//#define OPCODE_BLT 0x75
#define OPCODE_BGE 0x76
//#define OPCODE_BGT 0x87
///////////////////////////////////////////////////////////////////////////////


instruction_t opcode_arr[] = {
    instruction_t(OPCODE_LW,    "lw",     IDisp),
    instruction_t(OPCODE_SW,    "sw",     IDisp),
    instruction_t(OPCODE_BEQ,   "beq",    IBranchComp),
    instruction_t(OPCODE_BLT,   "blt",    IBranchComp),
    instruction_t(OPCODE_NOT,   "not",    INotRD),
    instruction_t(OPCODE_XOR,   "xor",    R),
    instruction_t(OPCODE_ADDI,  "addi",   I),     
    instruction_t(OPCODE_ADD,   "add",    R),
    instruction_t(OPCODE_SUB,   "sub",    R),
    instruction_t(OPCODE_SHRA,  "shra",   RSH), 
    instruction_t(OPCODE_SHL,   "shl",    RSH), 
    instruction_t(OPCODE_J,     "j",      J),
    instruction_t(OPCODE_JAL,   "jal",    J),
    instruction_t(OPCODE_JR,    "jr",     R1RS),
    instruction_t(OPCODE_OUTPUT,"output", R1RS),   
    instruction_t(OPCODE_INPUT, "input",  R1RD),
};

// for CPUs without a shift (such as ones that have a rotate instead), the 'ldia' (also known as the 'la' instruction) can use multiple successive adds to shift a register to encode a full 16-bit immediate.
// such code is emitted if the LDIA_SHIFT_WITH_ADDS macro below true, else the usual shl instruction is used
#define LDIA_SHIFT_WITH_ADDS 0

const int NUM_OPCODES = sizeof(opcode_arr)/sizeof(instruction_t);
const int OPCODE_BITS = 4;
const int REG_BITS = 3;
const int SHIFT_AMT_BITS = 3;
const int IMM_BITS = 6;
const int JMP_ADDR_BITS = 12;
const int ADDR_BITS = 16;
const int WORD_BITS = 16;

const int IMEM_SIZE = 1<<ADDR_BITS;
const int DMEM_SIZE = 1<<ADDR_BITS;

const int LINK_REG = 7;
const int ZERO_REG = 0;
const int DATA_BASE_ADDR = 16384;

const int WIDTH = WORD_BITS;
const int DEPTH = 1<<ADDR_BITS;
const int NUM_REGS = 1<<REG_BITS;
const int MIN_IMM = -(1<<(IMM_BITS-1));
const int MAX_IMM = (1<<(IMM_BITS-1))-1;
const int MAX_JMP_ADDR = (1<<JMP_ADDR_BITS)-1;
const int MAX_SHIFT_AMT = (1<<SHIFT_AMT_BITS)-1;

struct type_i
{
  signed imm : IMM_BITS;
  unsigned rt : REG_BITS;
  unsigned rs : REG_BITS;
  unsigned opcode : OPCODE_BITS;
};

struct type_r
{
  unsigned shamt : SHIFT_AMT_BITS;
  unsigned rd : REG_BITS;
  unsigned rt : REG_BITS;
  unsigned rs : REG_BITS;
  unsigned opcode : OPCODE_BITS;
};

struct type_j
{
  unsigned addr : JMP_ADDR_BITS;
  unsigned opcode : OPCODE_BITS;
};

union inst
{
  type_i itype;
  type_r rtype;
  type_j jtype;
  unsigned short bits;
};

#endif
