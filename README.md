# DotnetAuthentication API

This is a secure authentication API built with ASP.NET Core 8.0, providing user registration, login, email verification, password reset, and Google OAuth login functionality. It uses PostgreSQL as the database and is designed for deployment in a production environment with Docker.

## Overview

- **Purpose**: Provides a RESTful API for authentication services, integrated with a Vue frontend at `https://dotnetauthentication-ui.soben.me`.
- **Technology Stack**:
    - ASP.NET Core 8.0
    - Entity Framework Core (PostgreSQL)
    - JWT Authentication
    - Google OAuth 2.0
    - Docker for containerization

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) (for containerized deployment)
- [PostgreSQL 15](https://www.postgresql.org/download/)
- Node.js and npm (for frontend, if testing locally)

## Installation

### Local Development

1. **Clone the Repository**:
   ```bash
   git clone <your-repo-url>
   cd DotnetAuthentication