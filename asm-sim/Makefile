all: asm sim

asm: asm.cpp format.h
	g++ -g -o $@ $<

sim: sim.cpp format.h
	g++ -g -o $@ $<

example.imem.lgsim: example.s asm
	./asm example.s

clean:
	rm -f asm sim
