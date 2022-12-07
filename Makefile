# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help build clean test restore publish docker-build docker-run docker-stop \
        pack format lint analyze docs examples

# Variables
DOTNET := dotnet
PROJECT := skiasharp-chart-engine.csproj
SOLUTION := skiasharp-chart-engine.sln
BUILD_CONFIG := Release
OUTPUT_DIR := ./bin/$(BUILD_CONFIG)
DOCKER_IMAGE := skiasharp-chart-engine
DOCKER_TAG := latest

# Default target
help:
	@echo "SkiaSharp Chart Engine - Makefile"
	@echo "=================================="
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Available targets:"
	@echo "  build          Build the project"
	@echo "  rebuild        Clean and rebuild"
	@echo "  clean          Remove build artifacts"
	@echo "  restore        Restore NuGet packages"
	@echo "  test           Run tests"
	@echo "  pack           Create NuGet package"
	@echo "  publish        Publish to NuGet (requires API key)"
	@echo ""
	@echo "  docker-build   Build Docker image"
	@echo "  docker-run     Run Docker container"
	@echo "  docker-stop    Stop Docker container"
	@echo ""
	@echo "  format         Format code with dotnet format"
	@echo "  lint           Run code analysis"
	@echo "  analyze        Run static analysis"
	@echo ""
	@echo "  docs           Generate documentation"
	@echo "  examples       Build example projects"
	@echo ""
	@echo "  ci             Run all CI checks locally"
	@echo ""

# Build targets
build: restore
	$(DOTNET) build $(PROJECT) -c $(BUILD_CONFIG)

rebuild: clean build

clean:
	$(DOTNET) clean $(PROJECT)
	rm -rf $(OUTPUT_DIR)
	rm -rf ./bin ./obj

restore:
	$(DOTNET) restore $(PROJECT)

# Test targets
test: build
	$(DOTNET) test $(PROJECT) -c $(BUILD_CONFIG) --no-build

# Package targets
pack: build
	$(DOTNET) pack $(PROJECT) -c $(BUILD_CONFIG) -o ./nupkg

publish: pack
	@if [ -z "$(NUGET_API_KEY)" ]; then \
		echo "Error: NUGET_API_KEY environment variable not set"; \
		exit 1; \
	fi
	$(DOTNET) nuget push ./nupkg/*.nupkg \
		--api-key $(NUGET_API_KEY) \
		--source https://api.nuget.org/v3/index.json \
		--skip-duplicate

# Docker targets
docker-build:
	docker build -t $(DOCKER_IMAGE):$(DOCKER_TAG) .
	@echo "Docker image built: $(DOCKER_IMAGE):$(DOCKER_TAG)"

docker-run: docker-build
	docker run -d -p 5000:5000 \
		--name $(DOCKER_IMAGE) \
		-e ASPNETCORE_URLS=http://+:5000 \
		-e CACHE_DURATION=300 \
		$(DOCKER_IMAGE):$(DOCKER_TAG)
	@echo "Container running on http://localhost:5000"

docker-stop:
	docker stop $(DOCKER_IMAGE) || true
	docker rm $(DOCKER_IMAGE) || true
	@echo "Container stopped and removed"

docker-compose-up:
	docker-compose up -d
	@echo "Services started (docker-compose)"

docker-compose-down:
	docker-compose down
	@echo "Services stopped (docker-compose)"

# Code quality targets
format:
	$(DOTNET) format $(PROJECT)
	@echo "Code formatted"

lint:
	$(DOTNET) format $(PROJECT) --verify-no-changes --verbosity diagnostic

analyze:
	$(DOTNET) build $(PROJECT) -c $(BUILD_CONFIG) /p:TreatWarningsAsErrors=true

# Documentation targets
docs:
	@echo "Documentation is in ./docs directory"
	@ls -la ./docs/

examples:
	@echo "Building examples..."
	@for file in examples/*.cs; do \
		echo "Found example: $$file"; \
	done

# CI targets (run locally)
ci: clean restore build test lint analyze
	@echo "✓ All CI checks passed"

# Development workflow
dev-setup: restore
	@echo "Development environment ready"
	@echo "Run 'make build' to build the project"
	@echo "Run 'make test' to run tests"

watch:
	$(DOTNET) watch build $(PROJECT) -c $(BUILD_CONFIG)

# Release targets
release-check: clean restore build test lint analyze
	@echo "✓ Project is ready for release"

release-version:
	@echo "Current version:"
	@grep -A 1 '<Version>' $(PROJECT)

# Utility targets
info:
	$(DOTNET) --version
	$(DOTNET) --info

list-packages:
	$(DOTNET) list package

update-packages:
	$(DOTNET) add package --update-all

# Git hooks setup
setup-hooks:
	@echo "Setting up git hooks..."
	@chmod +x .git/hooks/pre-commit
	@echo "Git hooks configured"

# Performance testing
benchmark:
	@echo "Running performance benchmarks..."
	@echo "Note: Run examples for performance data"

# Clean up everything
distclean: clean docker-stop
	rm -rf ./nupkg
	rm -rf ./output
	rm -rf ./.vs
	rm -rf ./.vscode
	@echo "Complete cleanup done"

# All in one targets
all: ci pack
	@echo "✓ Build complete"

.PHONY: help build rebuild clean restore test pack publish \
        docker-build docker-run docker-stop docker-compose-up docker-compose-down \
        format lint analyze docs examples ci dev-setup watch \
        release-check release-version info list-packages update-packages \
        setup-hooks benchmark distclean all
