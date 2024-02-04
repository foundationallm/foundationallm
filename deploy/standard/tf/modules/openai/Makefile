default: test

testdir := ./test
example ?= default

docs:
	terraform-docs markdown document --output-file README.md --output-mode inject .

tffmt:
	terraform fmt -recursive

gofmt:
	cd $(testdir) && go fmt

fmt: tffmt gofmt

tidy:
	cd $(testdir) && go mod tidy

# Example: make test
test: tidy fmt docs
	cd $(testdir) && go test -v --timeout=60m

# Example: make upgrade
upgrade: fmt docs
	cd ./examples/$(example) && terraform init -upgrade

# Example: make deploy
deploy: fmt docs
	cd ./examples/$(example) && terraform apply

# Example: make destroy
destroy: fmt docs
	cd ./examples/$(example) && terraform destroy
