# E-Commerce Application

A full-stack e-commerce application with product browsing, shopping cart, favorites, and admin product management.

## 🚀 Live Demo

- **Frontend**: [https://codee-commerce.netlify.app](https://codee-commerce.netlify.app)
- **Backend API**: [https://e-commerce-production-3659.up.railway.app](https://e-commerce-production-3659.up.railway.app)

## ✨ Features

- Browse and view products
- Shopping cart functionality
- Save favorite products
- User registration and login
- Admin panel for product management
- Order processing

## 🛠 Tech Stack

- **Frontend**: Angular 20.3, TypeScript, Bootstrap
- **Backend**: ASP.NET Core 9.0, C#
- **Database**: PostgreSQL
- **Hosting**: Netlify (Frontend), Railway (Backend)

## 📁 Project Structure

```
Hemnet2/
├── frontend/          # Angular application
└── backend/           # ASP.NET Core API
```

## 🚀 Quick Start

### Backend

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend

```bash
cd frontend
npm install
npm start
```

## 📡 Main API Endpoints

- `GET /products` - Get all products
- `POST /auth/login` - User login
- `POST /auth/register` - User registration
- `GET /cart` - Get shopping cart
- `GET /favorites` - Get favorite products
- `POST /orders` - Create order

## 🚢 Deployment

- **Backend**: Deployed on Railway with Docker
- **Frontend**: Deployed on Netlify

## 👤 Author

Adam Kosovic
