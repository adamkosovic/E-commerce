# E-Commerce Application

A full-stack e-commerce application built with Angular (frontend) and ASP.NET Core (backend), featuring product management, shopping cart, favorites, user authentication, and order processing.

## 🚀 Live Demo

- **Frontend**: [https://codee-commerce.netlify.app](https://codee-commerce.netlify.app)
- **Backend API**: [https://e-commerce-production-3659.up.railway.app](https://e-commerce-production-3659.up.railway.app)

## 📋 Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Deployment](#deployment)
- [Environment Configuration](#environment-configuration)

## ✨ Features

### Customer Features
- **Product Browsing**: View all products with images, descriptions, and prices
- **Product Details**: Detailed product pages with full information
- **Shopping Cart**: Add/remove items, update quantities, view cart total
- **Favorites**: Save favorite products for quick access
- **User Authentication**: Register and login with JWT-based authentication
- **Order Management**: Create and view orders with tax calculation (25% VAT)
- **Checkout Process**: Multi-step checkout (cart → shipping → address → payment)

### Admin Features
- **Product Management**: Create, read, update, and delete products
- **Admin Dashboard**: Protected admin routes with role-based access control
- **Order Viewing**: View all customer orders

## 🛠 Tech Stack

### Frontend
- **Framework**: Angular 20.3
- **Language**: TypeScript
- **Styling**: CSS, Bootstrap 5.3
- **State Management**: RxJS Observables
- **HTTP Client**: Angular HttpClient
- **Routing**: Angular Router with guards

### Backend
- **Framework**: ASP.NET Core 9.0
- **Language**: C#
- **Database**: PostgreSQL (via Entity Framework Core)
- **Authentication**: JWT Bearer Tokens
- **ORM**: Entity Framework Core 9.0
- **API Documentation**: Swagger/OpenAPI

### Infrastructure
- **Frontend Hosting**: Netlify
- **Backend Hosting**: Railway
- **Database**: PostgreSQL (Railway)
- **Containerization**: Docker

## 📁 Project Structure

```
Hemnet2/
├── frontend/                 # Angular application
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/   # Reusable components
│   │   │   │   ├── cart/
│   │   │   │   ├── favorite-products/
│   │   │   │   ├── navbar/
│   │   │   │   ├── product-detail/
│   │   │   │   └── product-list/
│   │   │   ├── pages/        # Page components
│   │   │   │   ├── admin/
│   │   │   │   ├── home/
│   │   │   │   ├── login/
│   │   │   │   ├── payment/
│   │   │   │   ├── register/
│   │   │   │   └── shop/
│   │   │   ├── services/      # Angular services
│   │   │   ├── guards/       # Route guards
│   │   │   └── interceptors/ # HTTP interceptors
│   │   └── environments/     # Environment configs
│   └── angular.json
│
└── backend/                  # ASP.NET Core API
    ├── Controllers/          # API controllers
    │   ├── AuthController.cs
    │   ├── CartController.cs
    │   ├── FavoritesController.cs
    │   ├── OrdersController.cs
    │   └── ProductsController.cs
    ├── Models/               # Data models
    ├── Data/                 # DbContext
    ├── Services/             # Business logic
    ├── Migrations/           # Database migrations
    ├── Dockerfile            # Docker configuration
    └── Program.cs            # Application entry point
```

## 🚀 Getting Started

### Prerequisites

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 18+** and **npm** - [Download](https://nodejs.org/)
- **PostgreSQL** (for local development) - [Download](https://www.postgresql.org/download/)
- **Angular CLI** - Install with `npm install -g @angular/cli`

### Backend Setup

1. **Navigate to backend directory**:
   ```bash
   cd backend
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure database connection**:
   - Update `appsettings.json` or `appsettings.Development.json` with your PostgreSQL connection string:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Host=localhost;Port=5432;Database=ecommerce;Username=postgres;Password=yourpassword"
     }
   }
   ```

4. **Run database migrations**:
   ```bash
   dotnet ef database update
   ```

5. **Run the backend**:
   ```bash
   dotnet run
   ```
   - Backend will be available at `https://localhost:7271`
   - Swagger UI at `https://localhost:7271/swagger`

### Frontend Setup

1. **Navigate to frontend directory**:
   ```bash
   cd frontend
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Configure environment**:
   - For local development, `environment.ts` is already configured
   - The proxy configuration in `proxy.conf.json` will route API calls to the backend

4. **Run the frontend**:
   ```bash
   npm start
   ```
   - Frontend will be available at `http://localhost:4200`

## 📡 API Endpoints

### Authentication
- `POST /auth/register` - Register a new user
- `POST /auth/login` - Login and receive JWT token

### Products
- `GET /products` - Get all products (public)
- `GET /products/{id}` - Get product by ID (public)
- `POST /products` - Create product (Admin only)
- `PUT /products/{id}` - Update product (Admin only)
- `DELETE /products/{id}` - Delete product (Admin only)

### Cart
- `GET /cart` - Get user's cart (authenticated)
- `POST /cart/items` - Add item to cart (authenticated)
- `PUT /cart/items/{id}` - Update cart item quantity (authenticated)
- `DELETE /cart/items/{id}` - Remove item from cart (authenticated)
- `DELETE /cart` - Clear entire cart (authenticated)

### Favorites
- `GET /favorites` - Get user's favorite products (authenticated)
- `POST /favorites/{productId}` - Add product to favorites (authenticated)
- `DELETE /favorites/{productId}` - Remove product from favorites (authenticated)

### Orders
- `POST /orders` - Create a new order (authenticated)
- `GET /orders` - Get user's orders (authenticated)

### Health & Testing
- `GET /health` - Health check endpoint
- `GET /healthz` - Alternative health check
- `GET /` - Root endpoint with API status
- `GET /api-test` - Simple API test endpoint
- `GET /db-test` - Database connectivity test

## 🚢 Deployment

### Backend (Railway)

1. **Connect your GitHub repository** to Railway
2. **Set root directory** to `/backend`
3. **Configure environment variables**:
   - `DATABASE_URL` - PostgreSQL connection string (auto-provided by Railway)
   - `PORT` - Set to `8080` (or leave blank for auto-detection)
   - `ASPNETCORE_ENVIRONMENT` - Set to `Production`
4. **Set build command**: Railway will auto-detect the Dockerfile
5. **Set start command**: Railway will use the Dockerfile's ENTRYPOINT
6. **Configure networking**:
   - Target Port: `8080`
   - Health Check Path: `/health`

### Frontend (Netlify)

1. **Connect your GitHub repository** to Netlify
2. **Set build settings**:
   - Base directory: `frontend`
   - Build command: `npm run build`
   - Publish directory: `frontend/dist/frontend/browser`
3. **Environment variables** (if needed):
   - Production builds automatically use `environment.prod.ts`
4. **Update CORS in backend**:
   - After deploying, update the Netlify URL in `backend/Program.cs` CORS configuration

## ⚙️ Environment Configuration

### Backend Environment Variables

```bash
# Database
DATABASE_URL=postgresql://user:password@host:port/database

# Server
PORT=8080
HTTP_PORTS=8080
ASPNETCORE_ENVIRONMENT=Production

# JWT (configure in appsettings.json)
JWT__Key=your-secret-key-here
JWT__Issuer=backend
JWT__Audience=backend
```

### Frontend Environment Files

**`environment.ts`** (Development):
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7271'
};
```

**`environment.prod.ts`** (Production):
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://e-commerce-production-3659.up.railway.app'
};
```

## 🔐 Authentication

The application uses **JWT (JSON Web Tokens)** for authentication:

1. User registers/logs in via `/auth/register` or `/auth/login`
2. Backend returns a JWT token
3. Frontend stores the token in `localStorage`
4. All authenticated requests include the token in the `Authorization` header: `Bearer <token>`
5. Backend validates the token and extracts user information

### User Roles
- **Customer**: Can browse products, manage cart, create orders
- **Admin**: All customer permissions + product management

## 🗄 Database Schema

### Main Tables
- **Products**: Product catalog
- **Users**: User accounts with email, password hash, and role
- **Carts**: Shopping carts (one per user)
- **CartItems**: Items in shopping carts
- **Orders**: Customer orders
- **OrderItems**: Items in orders
- **FavoriteProducts**: User's favorite products

## 🐳 Docker

The backend includes a `Dockerfile` for containerized deployment:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# ... build steps ...

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
# ... runtime configuration ...
```

## 📝 Development Notes

### CORS Configuration
- CORS is configured to allow requests from the Netlify frontend
- Update `WithOrigins()` in `Program.cs` if the frontend URL changes

### Database Migrations
- Migrations run automatically on application startup
- Use `dotnet ef migrations add <MigrationName>` to create new migrations
- Use `dotnet ef database update` to apply migrations

### Port Configuration
- Backend listens on port 8080 (configurable via `PORT` environment variable)
- Frontend runs on port 4200 in development
- Railway automatically sets the `PORT` environment variable

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is private and proprietary.

## 👤 Author

Adam Kosovic

---

**Note**: Make sure to update the CORS configuration in `backend/Program.cs` whenever you change the frontend URL, and update `frontend/src/environments/environment.prod.ts` if the backend URL changes.

