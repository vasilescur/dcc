#include "samples/io.c"

int loopCounter = 0;

int main() {
    #pragma OutString test.c
    
    #pragma OutString Hello, world!

    #pragma OutString Testing nested expression: putc(48 + (3 + (1 - 2))); 

    putc(48 + (3 + (1 - 2)));

    putc(10);

    #pragma OutString Testing conditionals (if statements)...
    #pragma OutString Should output "Y" since 1 == 1 is true:

    if (1 == 1) {
        putc('Y');
        putc(10);
    }

    #pragma OutString Should *NOT* output "N", since 1 == 2 is false:

    if (1 == 2) {
        putc('N');
        putc(10);
    }

    #pragma OutString Testing some math...

    putc('0' + (1 + 6) - 2 + (4 + 1) - 3);

    putc(10);
    return 0;
}