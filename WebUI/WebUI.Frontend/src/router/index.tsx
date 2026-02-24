/**
 * Router Configuration
 * 路由配置
 */

import { createHashRouter, Navigate } from 'react-router-dom';
import { isAuthenticated } from '../utils/tokenManager';

// Lazy load pages for code splitting
// 懒加载页面以实现代码分割
import React, { Suspense, lazy } from 'react';
import { Spin } from 'antd';

// Loading component
const PageLoading = () => (
  <div style={{ 
    display: 'flex', 
    justifyContent: 'center', 
    alignItems: 'center', 
    height: '100vh' 
  }}>
    <Spin size="large" tip="加载中..." />
  </div>
);

// Lazy load layout
const MainLayout = lazy(() => import('../components/layout/MainLayout'));

// Lazy load pages
const LoginPage = lazy(() => import('../pages/LoginPage'));
const DashboardPage = lazy(() => import('../pages/DashboardPage'));
const ChangePasswordPage = lazy(() => import('../pages/ChangePasswordPage'));
const NotificationsPage = lazy(() => import('../pages/NotificationsPage'));
const HelpPage = lazy(() => import('../pages/HelpPage'));
const NotFoundPage = lazy(() => import('../pages/NotFoundPage'));

// Trading pages
const StockTradingPage = lazy(() => import('../pages/StockTradingPage'));
const ETFTradingPage = lazy(() => import('../pages/ETFTradingPage'));
const OrdersPage = lazy(() => import('../pages/OrdersPage'));
const RecurringInvestmentPage = lazy(() => import('../pages/RecurringInvestmentPage'));

// Portfolio pages
const PositionsPage = lazy(() => import('../pages/PositionsPage'));
const PositionDetailPage = lazy(() => import('../pages/PositionDetailPage'));
const PortfolioAnalysisPage = lazy(() => import('../pages/PortfolioAnalysisPage'));

// Strategy pages
const StrategiesPage = lazy(() => import('../pages/StrategiesPage'));
const CreateStrategyPage = lazy(() => import('../pages/CreateStrategyPage'));
const EditStrategyPage = lazy(() => import('../pages/EditStrategyPage'));
const StrategyDetailPage = lazy(() => import('../pages/StrategyDetailPage'));

// Backtest pages
const BacktestsPage = lazy(() => import('../pages/BacktestsPage'));
const BacktestConfigPage = lazy(() => import('../pages/BacktestConfigPage'));
const BacktestResultPage = lazy(() => import('../pages/BacktestResultPage'));
const CompareBacktestsPage = lazy(() => import('../pages/CompareBacktestsPage'));
const OptimizationPage = lazy(() => import('../pages/OptimizationPage'));

// Risk pages
const RiskConfigPage = lazy(() => import('../pages/RiskConfigPage'));
const RiskDashboardPage = lazy(() => import('../pages/RiskDashboardPage'));

// Settings pages
const SettingsPage = lazy(() => import('../pages/SettingsPage'));

/**
 * Protected Route wrapper
 * 受保护路由包装器
 */
interface ProtectedRouteProps {
  children: React.ReactNode;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }
  
  return <>{children}</>;
};

/**
 * Public Route wrapper (redirect to dashboard if already authenticated)
 * 公开路由包装器（如果已认证则重定向到仪表板）
 */
interface PublicRouteProps {
  children: React.ReactNode;
}

const PublicRoute: React.FC<PublicRouteProps> = ({ children }) => {
  if (isAuthenticated()) {
    return <Navigate to="/dashboard" replace />;
  }
  
  return <>{children}</>;
};

/**
 * Router configuration
 * 路由配置
 */
export const router = createHashRouter([
  // Public routes
  {
    path: '/login',
    element: (
      <Suspense fallback={<PageLoading />}>
        <PublicRoute>
          <LoginPage />
        </PublicRoute>
      </Suspense>
    ),
  },
  
  // Protected routes with MainLayout
  {
    path: '/',
    element: (
      <Suspense fallback={<PageLoading />}>
        <ProtectedRoute>
          <MainLayout />
        </ProtectedRoute>
      </Suspense>
    ),
    children: [
      {
        index: true,
        element: <Navigate to="/dashboard" replace />,
      },
      {
        path: 'dashboard',
        element: <DashboardPage />,
      },
      {
        path: 'change-password',
        element: <ChangePasswordPage />,
      },
      {
        path: 'notifications',
        element: <NotificationsPage />,
      },
      {
        path: 'help',
        element: <HelpPage />,
      },
      // Trading pages
      {
        path: 'trading',
        children: [
          {
            path: 'stocks',
            element: <StockTradingPage />,
          },
          {
            path: 'stocks/:symbol',
            element: <StockTradingPage />,
          },
          {
            path: 'etfs',
            element: <ETFTradingPage />,
          },
          {
            path: 'recurring',
            element: <RecurringInvestmentPage />,
          },
        ],
      },
      {
        path: 'portfolio',
        children: [
          {
            path: 'positions',
            element: <PositionsPage />,
          },
          {
            path: 'positions/:symbol',
            element: <PositionDetailPage />,
          },
          {
            path: 'analysis',
            element: <PortfolioAnalysisPage />,
          },
          {
            path: 'allocation',
            element: <PortfolioAnalysisPage />,
          },
        ],
      },
      {
        path: 'orders',
        children: [
          {
            path: 'active',
            element: <OrdersPage />,
          },
          {
            path: 'history',
            element: <OrdersPage />,
          },
        ],
      },
      {
        path: 'strategies',
        children: [
          {
            index: true,
            element: <StrategiesPage />,
          },
          {
            path: 'list',
            element: <StrategiesPage />,
          },
          {
            path: 'create',
            element: <CreateStrategyPage />,
          },
          {
            path: ':id',
            element: <StrategyDetailPage />,
          },
          {
            path: ':id/edit',
            element: <EditStrategyPage />,
          },
        ],
      },
      {
        path: 'backtests',
        children: [
          {
            index: true,
            element: <BacktestsPage />,
          },
          {
            path: 'list',
            element: <BacktestsPage />,
          },
          {
            path: 'new',
            element: <BacktestConfigPage />,
          },
          {
            path: ':id',
            element: <BacktestResultPage />,
          },
          {
            path: 'compare',
            element: <CompareBacktestsPage />,
          },
          {
            path: 'optimize',
            element: <OptimizationPage />,
          },
        ],
      },
      {
        path: 'risk',
        children: [
          {
            path: 'dashboard',
            element: <RiskDashboardPage />,
          },
          {
            path: 'config',
            element: <RiskConfigPage />,
          },
        ],
      },
      {
        path: 'settings',
        children: [
          {
            index: true,
            element: <SettingsPage />,
          },
          {
            path: 'account',
            element: <SettingsPage />,
          },
          {
            path: 'ibkr',
            element: <SettingsPage />,
          },
          {
            path: 'system',
            element: <SettingsPage />,
          },
        ],
      },
    ],
  },
  
  // 404 Not Found
  {
    path: '*',
    element: (
      <Suspense fallback={<PageLoading />}>
        <NotFoundPage />
      </Suspense>
    ),
  },
]);

export default router;
