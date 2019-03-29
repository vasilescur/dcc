#include <cstdlib>
#include <cstring>
#include <iostream>
#include <iomanip>
#include <fstream>
#include <string>
#include <sstream>
#include <vector>
#include <stdint.h>
#include <limits.h>
#include "format.h"
#include <errno.h>


#ifdef __linux__
  #include <termios.h>
#endif

using namespace std;

const int MEM_SIZE = DEPTH;
unsigned short reg[NUM_REGS];
unsigned short imem[MEM_SIZE];
unsigned short dmem[MEM_SIZE];
//unsigned short mem[MEM_SIZE];

void usage() { 
  cerr << "Usage:" << endl
       << "  sim [options] <myprogram.imem.lgsim> <myprogram.dmem.lgsim>" << endl
       << endl
       << "Options:" << endl
       << "  -v: verbose output" << endl
       << "  -F: cause every input instruction to yield no data instead of reading a real keystroke" << endl
       << "  -n: print the number of dynamic instructions executed at the end" << endl
       << endl;
  exit(0);
}
string insn2str(inst insn);

unsigned short rotl32 (unsigned short value, unsigned short count) {
    const unsigned short mask = (CHAR_BIT*sizeof(value)-1);
    count &= mask;
    return (value<<count) | (value>>( (-count) & mask ));
}

unsigned short rotr32 (unsigned short value, unsigned short count) {
    const unsigned short mask = (CHAR_BIT*sizeof(value)-1);
    count &= mask;
    return (value>>count) | (value<<( (-count) & mask ));
}

void show_regs() {
  cout << "Regs: [";
  for(int i = 0; i < NUM_REGS; i++) {
    cout << hex << setw((WORD_BITS+3)/4) << setfill('0') << reg[i] << dec;
    if (i<NUM_REGS-1) cout  << ' ';
  }
  cout << "]  ";
}

// returns number of words read, or -1 on error
int read_lgsim_file(string filename, unsigned short* mem) {
    ifstream file(filename.c_str());
    if(!file) {
        cerr << filename << ": " << strerror(errno) << endl;
        return -1;
    }
  
    string line;
    int lineno = 1;
    if (!getline(file,line)) {
        cerr << filename << ": Couldn't read start of file." << endl;
        return -1;
    }
    lineno++;
    if (line != "v2.0 raw") {
        cerr << filename << ":" << lineno << ": File does not contain expected Logisim data file header ('v2.0 raw')." << endl;
        return -1;
    }
    unsigned short pc = 0;
    unsigned short word;
    while(getline(file,line)) {
        istringstream in(line);
        if (!(in>>hex>>word)) {
            cerr << filename << ":" << lineno << ": Error reading word" << endl;
            return -1;
        }
        mem[pc++] = word;
        lineno++;
    }
    return pc;
}

int main(int argc, char** argv)
{
  bool verbose = false, insncount = false;
  bool inputs_fail = false;
  string imem_filename = "";
  string dmem_filename = "";
  
  for(int i = 1; i < argc; i++)
  {
    if(strcmp(argv[i],"-v") == 0)
      verbose = insncount = true;
    else if(strcmp(argv[i],"-n") == 0)
      insncount = true;
    else if(strcmp(argv[i],"-F") == 0)
      inputs_fail = true;
    else if(imem_filename == "")
      imem_filename = argv[i];
    else if(dmem_filename == "")
      dmem_filename = argv[i];
    else
      usage();
  }

  if(imem_filename == "" || dmem_filename == "")
    usage();

  if (read_lgsim_file(imem_filename,imem)<0) return 1;
  if (read_lgsim_file(dmem_filename,dmem)<0) return 1;

  #ifdef __linux__
    termios old_termios_settings,new_termios_settings;
    tcgetattr(0,&old_termios_settings);
    new_termios_settings = old_termios_settings;
    new_termios_settings.c_lflag &= (~ICANON);
    new_termios_settings.c_lflag &= (~ECHO);
    new_termios_settings.c_cc[VTIME] = 0;
    new_termios_settings.c_cc[VMIN] = 1;
    tcsetattr(0,TCSANOW,&new_termios_settings);
  #endif

  long long count = 0;
  int pc = 0;
  bool quit = false;
  while(!quit)
  {
    inst insn;
    insn.bits = imem[pc];

    reg[ZERO_REG] = 0;    
    
    if(verbose)
    {
      show_regs();
      cout << hex << setw((ADDR_BITS+3)/4) << setfill('0') << pc << ' ' << setw((ADDR_BITS+3)/4) << setfill('0') << insn.bits << dec;
      cout << ' ' << insn2str(insn) << endl;
    }

    pc++;
    count++;
    
    if (pc >= IMEM_SIZE) {
        cerr << "Tried to execute an instruction with PC bigger than the size of memory, aborting." << endl;
        quit = true;
    }

    switch((char)insn.itype.opcode) // the cast is just to suppress warnings about the unsupported opcodes
    {
      case OPCODE_ADD:
        reg[insn.rtype.rd] = reg[insn.rtype.rs] + reg[insn.rtype.rt];
        break;
      case OPCODE_ADDI:
        reg[insn.itype.rt] = reg[insn.itype.rs] + insn.itype.imm;
        break;
      case OPCODE_SUB:
        reg[insn.rtype.rd] = reg[insn.rtype.rs] - reg[insn.rtype.rt];
        break;
      case OPCODE_NOT:
        reg[insn.rtype.rd] = ~reg[insn.rtype.rs];
        break;
      case OPCODE_NAND:
        reg[insn.rtype.rd] = ~(reg[insn.rtype.rs] & reg[insn.rtype.rt]);
        break;
      case OPCODE_XOR:
        reg[insn.rtype.rd] = reg[insn.rtype.rs] ^ reg[insn.rtype.rt];
        break;
      case OPCODE_OR:
        reg[insn.rtype.rd] = reg[insn.rtype.rs] | reg[insn.rtype.rt];
        break;
      case OPCODE_AND:
        reg[insn.rtype.rd] = reg[insn.rtype.rs] & reg[insn.rtype.rt];
        break;
      case OPCODE_ROL:
        reg[insn.rtype.rd] = rotl32(reg[insn.rtype.rs], insn.rtype.shamt);
        break;
      case OPCODE_ROR:
        reg[insn.rtype.rd] = rotr32(reg[insn.rtype.rs], insn.rtype.shamt);
        break;
      case OPCODE_SHL:
      case OPCODE_SLL:
        reg[insn.rtype.rd] = reg[insn.rtype.rs] << insn.rtype.shamt;
        break;
      case OPCODE_SHR:
      case OPCODE_SRL:
        reg[insn.rtype.rd] = reg[insn.rtype.rs] >> insn.rtype.shamt;
        break;
      case OPCODE_SHRA:
        if(reg[insn.rtype.rs] >> (WORD_BITS - 1) == 0) reg[insn.rtype.rd] = (reg[insn.rtype.rs] >> insn.rtype.shamt);
        else reg[insn.rtype.rd] = (short)reg[insn.rtype.rs] >> insn.rtype.shamt;
        break;
      case OPCODE_LW:
        reg[insn.itype.rt] = dmem[reg[insn.itype.rs] + insn.itype.imm];
        break;
      case OPCODE_SW:
        dmem[reg[insn.itype.rs] + insn.itype.imm] = reg[insn.itype.rt];
        break;
      case OPCODE_BNE:
        if(reg[insn.itype.rs] != reg[insn.itype.rt] && insn.itype.imm == -1)
        {
          // special encoding of the halt instruction
          quit = true;
          break;
        }
        if(reg[insn.itype.rs] != reg[insn.itype.rt])
          pc += insn.itype.imm;
        break;
      case OPCODE_BEQ:
        if(reg[insn.itype.rs]==0 && reg[insn.itype.rt]==0 && insn.itype.imm == -1)
        {
          // special encoding of the halt instruction
          quit = true;
          break;
        }
        if(reg[insn.itype.rs] == reg[insn.itype.rt])
          pc += insn.itype.imm;
        break;
      case OPCODE_BEQZ:
        if(reg[insn.itype.rs]==0 && insn.itype.imm == -1)
        {
          // special encoding of the halt instruction
          quit = true;
          break;
        }
        if(reg[insn.itype.rs] == reg[ZERO_REG])
          pc += insn.itype.imm;
        break;
      case OPCODE_BLT:
        if((short)reg[insn.itype.rs] <  (short)reg[insn.itype.rt])
          pc += insn.itype.imm;
        break;
      case OPCODE_BLE:
        if((short)reg[insn.itype.rs] <= (short)reg[insn.itype.rt])
          pc += insn.itype.imm;
        break;
      case OPCODE_BGT:
        if((short)reg[insn.itype.rs] >  (short)reg[insn.itype.rt])
          pc += insn.itype.imm;
        break;
      case OPCODE_BGE:
        if((short)reg[insn.itype.rs] >= (short)reg[insn.itype.rt])
          pc += insn.itype.imm;
        break;
      case OPCODE_J:
        pc = (pc & ~MAX_JMP_ADDR) | insn.jtype.addr;
        break;
      case OPCODE_JR:
        pc = reg[insn.rtype.rs];
        break;
      case OPCODE_JAL:
        reg[LINK_REG] = pc;
        pc = (pc & ~MAX_JMP_ADDR) | insn.jtype.addr;
        break;
      case OPCODE_OUTPUT:
        cout << (reg[insn.rtype.rs] == 0xD ? '\n' : (char)reg[insn.rtype.rs]);
        break;
      case OPCODE_INPUT:
        reg[insn.rtype.rt] = inputs_fail ? 128 : getchar();
        break;
      default:
        cerr << "Invalid opcode " << hex << insn.itype.opcode << " at pc " << pc-1 << "!" << endl;
        quit = true;
        break;
    }

  }

  if(insncount)
      cerr << dec << count << " dynamic instructions executed" << endl;
  #ifdef __linux__
    tcsetattr(0,TCSANOW,&old_termios_settings);
  #endif
}

string insn2str(inst insn)
{
  int index = 0;
  for(int i = 0; i < NUM_OPCODES; i++)
    if(opcode_arr[i].opcode == insn.itype.opcode)
      index = i;

  ostringstream oss;
  oss << opcode_arr[index].str;
  switch(opcode_arr[index].type)
  {
    case I:
      oss << " $r" << insn.itype.rt << ",$r" << insn.itype.rs << ',' << (short)insn.itype.imm;
      break;
    case IBranch:
      oss << " $r" << insn.itype.rs << ",$r" << insn.itype.rt << ',' << (short)insn.itype.imm;
      break;
    case IBranchComp:
      oss << " $r" << insn.itype.rs << ",$r" << insn.itype.rt << ',' << (short)insn.itype.imm;
      break;
    case I1Branch:
      oss << " $r" << insn.itype.rs << ',' << (short)insn.itype.imm;
      break;
    case I1:
      oss << " $r" << insn.itype.rt << ',' << (short)insn.itype.imm;
      break;
    case IDisp:
      oss << " $r" << insn.itype.rt << ',' << (short)insn.itype.imm << "($r" << insn.itype.rs << ')';
      break;
    case INotRD:
      oss << " $r" << insn.rtype.rd << ',' << "$r" << insn.rtype.rs;
      break;
    case INotRT:
      oss << " $r" << insn.rtype.rt << ',' << "$r" << insn.rtype.rs;
      break;
    case J:
      oss << ' ' << insn.jtype.addr;
      break;
    case R:
      oss << " $r" << insn.rtype.rd << ",$r" << insn.rtype.rs << ",$r" << insn.rtype.rt;
      break;
    case R0:
      break;
    case R1RS:
      oss << " $r" << insn.rtype.rs;
      break;
    case R1RT:
      oss << " $r" << insn.rtype.rt;
      break;
    case R1RD:
      oss << " $r" << insn.rtype.rd;
      break;
    case RDRS:
      oss << " $r" << insn.rtype.rd << ",$r" << insn.rtype.rs;
      break;
    case RDRT:
      oss << " $r" << insn.rtype.rd << ",$r" << insn.rtype.rt;
      break;
    case RSH:
      oss << " $r" << insn.rtype.rd << ",$r" << insn.itype.rs << ',' << (short)insn.rtype.shamt;
      break;
    default:
      break;
  }

  return oss.str();
}
