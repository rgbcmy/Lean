/**
 * Login Page Component Tests
 * 登录页面组件测试
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import LoginPage from '@/pages/LoginPage';
import * as authApi from '@/api/authApi';
import { useAuthStore } from '@/stores';

// Mock dependencies
vi.mock('@/api/authApi');
vi.mock('@/stores', () => ({
  useAuthStore: vi.fn()
}));

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate
  };
});

describe('LoginPage Component', () => {
  const mockSetAuth = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    (useAuthStore as any).mockReturnValue(mockSetAuth);
  });

  const renderLoginPage = () => {
    return render(
      <BrowserRouter>
        <LoginPage />
      </BrowserRouter>
    );
  };

  it('should render login form with all fields', () => {
    renderLoginPage();

    expect(screen.getByText('Lean WebUI')).toBeInTheDocument();
    expect(screen.getByLabelText(/用户名/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/密码/i)).toBeInTheDocument();
    expect(screen.getByRole('checkbox')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /登录/i })).toBeInTheDocument();
  });

  it('should show validation errors for empty fields', async () => {
    const user = userEvent.setup();
    renderLoginPage();

    const loginButton = screen.getByRole('button', { name: /登录/i });
    await user.click(loginButton);

    await waitFor(() => {
      expect(screen.getByText(/请输入用户名/i)).toBeInTheDocument();
      expect(screen.getByText(/请输入密码/i)).toBeInTheDocument();
    });
  });

  it('should call login API with correct credentials', async () => {
    const user = userEvent.setup();
    const mockLoginResponse = {
      accessToken: 'mock-access-token',
      refreshToken: 'mock-refresh-token',
      user: { id: '1', username: 'testuser' }
    };
    
    (authApi.login as any).mockResolvedValue(mockLoginResponse);
    (useAuthStore as any).mockReturnValue({
      setAuth: mockSetAuth
    });

    renderLoginPage();

    // Fill in form
    await user.type(screen.getByLabelText(/用户名/i), 'testuser');
    await user.type(screen.getByLabelText(/密码/i), 'password123');
    await user.click(screen.getByRole('checkbox'));

    // Submit form
    await user.click(screen.getByRole('button', { name: /登录/i }));

    await waitFor(() => {
      expect(authApi.login).toHaveBeenCalledWith({
        username: 'testuser',
        password: 'password123',
        rememberMe: true
      });
    });
  });

  it('should navigate to dashboard on successful login', async () => {
    const user = userEvent.setup();
    const mockLoginResponse = {
      accessToken: 'mock-access-token',
      refreshToken: 'mock-refresh-token',
      user: { id: '1', username: 'testuser' }
    };
    
    (authApi.login as any).mockResolvedValue(mockLoginResponse);
    (useAuthStore as any).mockReturnValue({
      setAuth: mockSetAuth
    });

    renderLoginPage();

    await user.type(screen.getByLabelText(/用户名/i), 'testuser');
    await user.type(screen.getByLabelText(/密码/i), 'password123');
    await user.click(screen.getByRole('button', { name: /登录/i }));

    await waitFor(() => {
      expect(mockSetAuth).toHaveBeenCalledWith(
        'mock-access-token',
        'mock-refresh-token',
        { id: '1', username: 'testuser' }
      );
      expect(mockNavigate).toHaveBeenCalledWith('/dashboard', { replace: true });
    });
  });

  it('should display error message on failed login', async () => {
    const user = userEvent.setup();
    const mockError = {
      response: {
        data: {
          message: '用户名或密码错误'
        }
      }
    };
    
    (authApi.login as any).mockRejectedValue(mockError);
    (useAuthStore as any).mockReturnValue({
      setAuth: mockSetAuth
    });

    renderLoginPage();

    await user.type(screen.getByLabelText(/用户名/i), 'wronguser');
    await user.type(screen.getByLabelText(/密码/i), 'wrongpassword');
    await user.click(screen.getByRole('button', { name: /登录/i }));

    await waitFor(() => {
      expect(screen.getByText('用户名或密码错误')).toBeInTheDocument();
    });
  });

  it('should disable login button while loading', async () => {
    const user = userEvent.setup();
    (authApi.login as any).mockImplementation(() => 
      new Promise(resolve => setTimeout(resolve, 1000))
    );

    renderLoginPage();

    await user.type(screen.getByLabelText(/用户名/i), 'testuser');
    await user.type(screen.getByLabelText(/密码/i), 'password123');

    const loginButton = screen.getByRole('button', { name: /登录/i });
    await user.click(loginButton);

    expect(loginButton).toBeDisabled();
  });
});
