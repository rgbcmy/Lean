/**
 * Theme Toggle Component Tests
 * 主题切换组件测试
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ThemeToggle from '@/components/ThemeToggle';
import { useThemeStore } from '@/stores';

// Mock the store
vi.mock('@/stores', () => ({
  useThemeStore: vi.fn()
}));

describe('ThemeToggle Component', () => {
  const mockToggleTheme = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render light theme button correctly', () => {
    (useThemeStore as any).mockReturnValue({
      theme: 'light',
      toggleTheme: mockToggleTheme
    });

    render(<ThemeToggle />);

    const button = screen.getByRole('button');
    expect(button).toBeInTheDocument();
    expect(button).toHaveTextContent('深色');
    expect(button).toHaveAttribute('title', '切换到深色模式');
  });

  it('should render dark theme button correctly', () => {
    (useThemeStore as any).mockReturnValue({
      theme: 'dark',
      toggleTheme: mockToggleTheme
    });

    render(<ThemeToggle />);

    const button = screen.getByRole('button');
    expect(button).toBeInTheDocument();
    expect(button).toHaveTextContent('浅色');
    expect(button).toHaveAttribute('title', '切换到浅色模式');
  });

  it('should call toggleTheme when button is clicked', async () => {
    const user = userEvent.setup();
    (useThemeStore as any).mockReturnValue({
      theme: 'light',
      toggleTheme: mockToggleTheme
    });

    render(<ThemeToggle />);

    const button = screen.getByRole('button');
    await user.click(button);

    expect(mockToggleTheme).toHaveBeenCalledTimes(1);
  });

  it('should apply custom styles when provided', () => {
    const customStyle = { margin: '10px', padding: '5px' };
    (useThemeStore as any).mockReturnValue({
      theme: 'light',
      toggleTheme: mockToggleTheme
    });

    render(<ThemeToggle style={customStyle} />);

    const button = screen.getByRole('button');
    expect(button).toHaveStyle({ margin: '10px', padding: '5px' });
  });
});
