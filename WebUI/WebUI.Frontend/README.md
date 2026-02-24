# Lean WebUI Frontend

Personal Trading Web Interface for Lean Algorithmic Trading Engine

## Features Implemented (Module 15 - Project Setup)

✅ **React Router Configuration**
- Browser router with lazy loading
- Protected routes (authentication required)
- Public routes (redirect if authenticated)
- 404 Not Found page

✅ **Ant Design Theme**
- Light and dark theme support
- Chinese localization (zh_CN)
- Custom theme configuration
- Theme toggle component

✅ **Axios HTTP Client**
- Base URL configuration from environment variables
- Request interceptor (adds Authorization header)
- Response interceptor (error handling)
- Automatic token refresh on 401
- Error message display

✅ **SignalR Real-time Communication**
- Market data hub connection
- Order hub connection
- Strategy hub connection
- Automatic reconnection with exponential backoff
- Topic-based subscriptions

✅ **Zustand State Management**
- Auth store (user, tokens)
- Connection store (IBKR, SignalR status)
- Theme store (light/dark preference)
- Persistence to localStorage

✅ **JWT Token Management**
- Token storage and retrieval
- Token expiration checking
- Automatic token refresh
- Auto-refresh timer (checks every 60 seconds)

✅ **Theme Switching**
- Light/dark theme toggle
- Preference saved to localStorage
- Smooth transitions

✅ **Environment Variables**
- API base URL configuration
- SignalR hub URL configuration
- Development and production environments

## Tech Stack

- **React 19** - UI framework
- **TypeScript** - Type safety
- **Vite 7** - Build tool
- **Ant Design 6** - UI component library
- **React Router 7** - Routing
- **Axios 1.13** - HTTP client
- **SignalR 10** - Real-time communication
- **Zustand 5** - State management
- **ECharts 6** - Data visualization (ready for use)

## Project Structure

```
src/
├── api/                    # API clients
│   ├── apiClient.ts       # Axios instance with interceptors
│   └── index.ts
├── components/            # Reusable components
│   ├── ThemeProvider.tsx  # Theme wrapper
│   └── ThemeToggle.tsx    # Theme switch button
├── config/                # Configuration
│   ├── env.ts            # Environment variables
│   ├── theme.ts          # Ant Design theme config
│   └── index.ts
├── pages/                 # Page components
│   ├── LoginPage.tsx     # Login page
│   ├── DashboardPage.tsx # Dashboard
│   └── NotFoundPage.tsx  # 404 page
├── router/                # Route configuration
│   └── index.tsx         # Router setup
├── services/              # Business logic services
│   ├── signalRService.ts # SignalR client
│   └── index.ts
├── stores/                # Zustand stores
│   ├── authStore.ts      # Authentication state
│   ├── connectionStore.ts # Connection status
│   ├── themeStore.ts     # Theme preference
│   └── index.ts
├── utils/                 # Utility functions
│   ├── tokenManager.ts   # JWT token utilities
│   └── index.ts
├── App.tsx               # Root component
├── main.tsx              # Entry point
├── App.css               # App styles
└── index.css             # Global styles
```

## Environment Configuration

Create `.env.development` and `.env.production` files:

```env
VITE_API_BASE_URL=https://localhost:5001
VITE_SIGNALR_HUB_URL=https://localhost:5001/hubs
```

## Getting Started

### Install Dependencies

```bash
npm install
```

### Development Server

```bash
npm run dev
```

### Build for Production

```bash
npm run build
```

### Preview Production Build

```bash
npm run preview
```

## Next Steps (Module 16+)

- [ ] Implement authentication pages (login, change password)
- [ ] Create main layout with navigation
- [ ] Implement trading pages
- [ ] Implement portfolio pages
- [ ] Implement strategy pages
- [ ] Add charts and visualizations

## Notes

- All UI text is in Chinese (中文界面)
- Date/time format follows Chinese conventions
- Theme preference persists across sessions
- JWT tokens auto-refresh before expiration
- SignalR connections auto-reconnect on disconnect
