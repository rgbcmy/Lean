import { useState, useEffect } from 'react';

export type ScreenSize = 'mobile' | 'tablet' | 'desktop';

export interface ResponsiveChartConfig {
  screenSize: ScreenSize;
  chartHeight: number;
  showLegend: boolean;
  showToolbox: boolean;
  fontSize: number;
  labelRotation: number;
}

/**
 * Hook to get responsive chart configuration based on screen size
 */
export function useResponsiveChart(baseHeight: number = 400): ResponsiveChartConfig {
  const [screenSize, setScreenSize] = useState<ScreenSize>(getScreenSize());

  useEffect(() => {
    const handleResize = () => {
      setScreenSize(getScreenSize());
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  // Configure based on screen size
  switch (screenSize) {
    case 'mobile':
      return {
        screenSize: 'mobile',
        chartHeight: Math.min(baseHeight, 300),
        showLegend: false, // Hide legend on mobile to save space
        showToolbox: false,
        fontSize: 10,
        labelRotation: 45,
      };
    case 'tablet':
      return {
        screenSize: 'tablet',
        chartHeight: baseHeight * 0.8,
        showLegend: true,
        showToolbox: true,
        fontSize: 12,
        labelRotation: 30,
      };
    case 'desktop':
    default:
      return {
        screenSize: 'desktop',
        chartHeight: baseHeight,
        showLegend: true,
        showToolbox: true,
        fontSize: 14,
        labelRotation: 0,
      };
  }
}

/**
 * Get current screen size category
 */
function getScreenSize(): ScreenSize {
  const width = window.innerWidth;
  
  if (width < 768) {
    return 'mobile';
  } else if (width < 1024) {
    return 'tablet';
  } else {
    return 'desktop';
  }
}

/**
 * Hook to observe element size changes
 */
export function useElementSize(ref: React.RefObject<HTMLElement>): {
  width: number;
  height: number;
} {
  const [size, setSize] = useState({ width: 0, height: 0 });

  useEffect(() => {
    if (!ref.current) return;

    const observer = new ResizeObserver((entries) => {
      const entry = entries[0];
      if (entry) {
        setSize({
          width: entry.contentRect.width,
          height: entry.contentRect.height,
        });
      }
    });

    observer.observe(ref.current);

    // Initial size
    const rect = ref.current.getBoundingClientRect();
    setSize({ width: rect.width, height: rect.height });

    return () => {
      observer.disconnect();
    };
  }, [ref]);

  return size;
}

/**
 * Get responsive grid configuration for charts
 */
export function getResponsiveGrid(screenSize: ScreenSize): {
  left: string;
  right: string;
  top: string;
  bottom: string;
} {
  switch (screenSize) {
    case 'mobile':
      return {
        left: '5%',
        right: '5%',
        top: '10%',
        bottom: '20%',
      };
    case 'tablet':
      return {
        left: '8%',
        right: '8%',
        top: '10%',
        bottom: '15%',
      };
    case 'desktop':
    default:
      return {
        left: '10%',
        right: '10%',
        top: '10%',
        bottom: '15%',
      };
  }
}

/**
 * Get responsive font sizes for different chart elements
 */
export function getResponsiveFontSizes(screenSize: ScreenSize): {
  title: number;
  legend: number;
  axis: number;
  tooltip: number;
} {
  switch (screenSize) {
    case 'mobile':
      return {
        title: 14,
        legend: 10,
        axis: 10,
        tooltip: 12,
      };
    case 'tablet':
      return {
        title: 16,
        legend: 12,
        axis: 11,
        tooltip: 13,
      };
    case 'desktop':
    default:
      return {
        title: 18,
        legend: 14,
        axis: 12,
        tooltip: 14,
      };
  }
}

/**
 * Check if device is touch-enabled
 */
export function isTouchDevice(): boolean {
  return 'ontouchstart' in window || navigator.maxTouchPoints > 0;
}
