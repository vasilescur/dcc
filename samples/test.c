#include "samples/io.c"

// This program prints the character "0" and a line 
// feed to the console.

int myTest = 3;
int* testPtr = 4906;
int uninit;

int main() {
    putc(48);   // "0"
    putc(10);   // "\n"

    return 0;
}