#include "samples/io.c"

int main() {
    int myVar = 48;
    putc(myVar);

    myVar = myVar + 1;
    putc(myVar);

    putc(10);
    putc('D');
    putc(10);
    return 0;
}