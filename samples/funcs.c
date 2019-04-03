#include "samples/io.c"

// This is a program designed to test functions.

void otherFunc() {
    putc('O');
    putc('t');
    putc(10);
    return;
}

int main() {
    putc('M');
    putc(10);

    otherFunc();

    putc('n');
    putc(10);

    return 0;
}