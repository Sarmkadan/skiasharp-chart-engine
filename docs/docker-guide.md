## Docker Guide

### Quick Start

1. Install Docker on your system
2. Build the Docker image: `docker build -t skiasharp-chart-engine:latest .`
3. Run the Docker container: `docker run -p 5000:5000 skiasharp-chart-engine:latest`

### Docker Compose

1. Install Docker Compose on your system
2. Create a `docker-compose.yml` file
3. Run the Docker Compose container: `docker-compose up`

### Environment Variables

* `CACHE_DURATION_SECONDS`: Cache duration in seconds
* `MAX_CONCURRENT_RENDERS`: Maximum number of concurrent renders

### Production Deployment

1. Configure Docker Compose for production
2. Deploy to a cloud provider or container orchestration platform