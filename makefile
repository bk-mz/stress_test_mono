all: run

compile:
	xbuild /v:q /p:Configuration=Release

copy_domain:
	cp domain_dll/bin/Release/* test_mono/bin/Release/.

run: compile copy_domain
	cd test_mono/bin/Release/ && mono --debug --llvm --gc=boehm test_mono.exe

run-sgen: compile copy_domain
	cd test_mono/bin/Release/ && mono --debug --llvm --gc=sgen test_mono.exe
