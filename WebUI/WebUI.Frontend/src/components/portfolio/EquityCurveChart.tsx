/**
 * Equity Curve Chart (Line Chart)
 * 账户收益曲线图
 */

import React, { useEffect, useRef } from 'react';
import * as echarts from 'echarts';
import type { EquityCurveData } from '../../types/position';

interface EquityCurveChartProps {
  data: EquityCurveData[];
  title?: string;
  height?: number;
}

/**
 * EquityCurveChart Component
 * Displays account equity curve over time
 * 显示账户净值随时间的变化曲线
 */
const EquityCurveChart: React.FC<EquityCurveChartProps> = ({
  data,
  title = '账户收益曲线',
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
    const dates = data.map((item) => item.date);
    const equityValues = data.map((item) => item.equity);

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
        trigger: 'axis',
        formatter: (params: any) => {
          const param = params[0];
          const dataItem = data[param.dataIndex];
          let tooltip = `${param.axisValue}<br/>`;
          tooltip += `账户净值: $${param.value.toLocaleString(undefined, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          })}<br/>`;
          if (dataItem.deposits > 0) {
            tooltip += `入金: $${dataItem.deposits.toFixed(2)}<br/>`;
          }
          if (dataItem.withdrawals > 0) {
            tooltip += `出金: $${dataItem.withdrawals.toFixed(2)}<br/>`;
          }
          return tooltip;
        },
      },
      grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        top: '15%',
        containLabel: true,
      },
      xAxis: {
        type: 'category',
        boundaryGap: false,
        data: dates,
        axisLabel: {
          rotate: 45,
        },
      },
      yAxis: {
        type: 'value',
        name: '净值 ($)',
        axisLabel: {
          formatter: (value: number) =>
            `$${value.toLocaleString(undefined, {
              minimumFractionDigits: 0,
              maximumFractionDigits: 0,
            })}`,
        },
      },
      series: [
        {
          name: '账户净值',
          type: 'line',
          smooth: true,
          symbol: 'circle',
          symbolSize: 6,
          sampling: 'lttb',
          itemStyle: {
            color: '#1890ff',
          },
          areaStyle: {
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              {
                offset: 0,
                color: 'rgba(24, 144, 255, 0.3)',
              },
              {
                offset: 1,
                color: 'rgba(24, 144, 255, 0.05)',
              },
            ]),
          },
          data: equityValues,
        },
      ],
      dataZoom: [
        {
          type: 'inside',
          start: 0,
          end: 100,
        },
        {
          start: 0,
          end: 100,
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

export default EquityCurveChart;
