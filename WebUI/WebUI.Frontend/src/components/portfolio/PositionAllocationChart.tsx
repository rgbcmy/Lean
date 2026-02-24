/**
 * Position Allocation Chart (Pie Chart)
 * 持仓配置饼图
 */

import React, { useEffect, useRef } from 'react';
import * as echarts from 'echarts';
import type { AllocationItem } from '../../types/position';

interface PositionAllocationChartProps {
  data: AllocationItem[];
  title?: string;
  height?: number;
}

/**
 * PositionAllocationChart Component
 * Displays portfolio allocation as a pie chart
 * 以饼图形式显示投资组合配置
 */
const PositionAllocationChart: React.FC<PositionAllocationChartProps> = ({
  data,
  title = '持仓配置',
  height = 400,
}) => {
  const chartRef = useRef<HTMLDivElement>(null);
  const chartInstance = useRef<echarts.ECharts | null>(null);

  useEffect(() => {
    if (!chartRef.current) return;

    // Initialize chart
    if (!chartInstance.current) {
      chartInstance.current = echarts.init(chartRef.current);
    }

    // Prepare data for ECharts
    const chartData = data.map((item) => ({
      value: item.value,
      name: `${item.symbol} (${item.percentage.toFixed(2)}%)`,
    }));

    // Chart options
    const options: echarts.EChartsOption = {
      title: {
        text: title,
        left: 'center',
        top: 20,
        textStyle: {
          fontSize: 16,
          fontWeight: 'bold',
        },
      },
      tooltip: {
        trigger: 'item',
        formatter: (params: any) => {
          return `${params.name}<br/>市值: $${params.value.toLocaleString(undefined, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          })}`;
        },
      },
      legend: {
        orient: 'vertical',
        left: 'left',
        top: 'middle',
      },
      series: [
        {
          type: 'pie',
          radius: ['40%', '70%'],
          center: ['60%', '50%'],
          avoidLabelOverlap: true,
          itemStyle: {
            borderRadius: 10,
            borderColor: '#fff',
            borderWidth: 2,
          },
          label: {
            show: true,
            formatter: '{b}\n{d}%',
          },
          emphasis: {
            label: {
              show: true,
              fontSize: 14,
              fontWeight: 'bold',
            },
          },
          data: chartData,
        },
      ],
    };

    chartInstance.current.setOption(options);

    // Handle window resize
    const handleResize = () => {
      chartInstance.current?.resize();
    };
    window.addEventListener('resize', handleResize);

    // Cleanup
    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, [data, title]);

  return <div ref={chartRef} style={{ width: '100%', height }} />;
};

export default PositionAllocationChart;
