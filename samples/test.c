#include "samples/io.c"

int main() {
    putc(48 + (3 + (1 - 2)));

    putc('a');

    putc(10);

    if (1 == 1) {
        putc('Y');
    }

    if (1 == 2) {
        putc('N');
    }

    putc(10);
    return 0;
}