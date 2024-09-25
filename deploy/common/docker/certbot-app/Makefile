default: buildx

buildx: pull prepare
	bash ./build.sh
	rm -v Function-Library.ps1

prepare:
	cp -v ../../scripts/Function-Library.ps1 .

pull:
	docker pull mcr.microsoft.com/vscode/devcontainers/base:0-bullseye
