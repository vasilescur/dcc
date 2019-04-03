#include "samples/io.c"

// This program tests loops!

int main() {
    int targetNum = 5;
    int i = 0;
    int value = '0';

    asm("loop: ");

    putc(value);
    putc(10);

    i = i + 1;
    value = value + 1;

    if (i == targetNum) {
        asm("j end");
    }

    asm("j loop");
    // putc('L');
    // putc(10);

    asm("end: ");
    return 0;
}