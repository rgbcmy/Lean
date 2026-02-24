/**
 * Chart export utilities for saving charts as images
 */

import type { ECharts } from 'echarts';
import type { IChartApi } from 'lightweight-charts';

/**
 * Export ECharts instance as PNG
 */
export function exportEChartAsPNG(chart: ECharts, filename: string = 'chart.png'): void {
  const url = chart.getDataURL({
    type: 'png',
    pixelRatio: 2, // Higher resolution
    backgroundColor: '#fff',
  });

  downloadImage(url, filename);
}

/**
 * Export ECharts instance as SVG
 */
export function exportEChartAsSVG(chart: ECharts, filename: string = 'chart.svg'): void {
  const svgStr = chart.renderToSVGString();
  const blob = new Blob([svgStr], { type: 'image/svg+xml' });
  const url = URL.createObjectURL(blob);

  downloadFile(url, filename);
  URL.revokeObjectURL(url);
}

/**
 * Export Lightweight Charts instance as PNG
 * Note: Lightweight Charts doesn't have built-in export, so we use canvas conversion
 */
export function exportLightweightChartAsPNG(
  container: HTMLDivElement,
  filename: string = 'chart.png'
): void {
  // Find the canvas element in the container
  const canvas = container.querySelector('canvas');
  if (!canvas) {
    console.error('Canvas not found in chart container');
    return;
  }

  // Convert to blob and download
  canvas.toBlob((blob) => {
    if (blob) {
      const url = URL.createObjectURL(blob);
      downloadImage(url, filename);
      URL.revokeObjectURL(url);
    }
  }, 'image/png');
}

/**
 * Copy chart image to clipboard (ECharts)
 */
export async function copyEChartToClipboard(chart: ECharts): Promise<void> {
  const url = chart.getDataURL({
    type: 'png',
    pixelRatio: 2,
    backgroundColor: '#fff',
  });

  try {
    const response = await fetch(url);
    const blob = await response.blob();
    
    await navigator.clipboard.write([
      new ClipboardItem({
        'image/png': blob,
      }),
    ]);

    console.log('Chart copied to clipboard');
  } catch (error) {
    console.error('Failed to copy chart to clipboard:', error);
    throw error;
  }
}

/**
 * Copy chart image to clipboard (Lightweight Charts)
 */
export async function copyLightweightChartToClipboard(container: HTMLDivElement): Promise<void> {
  const canvas = container.querySelector('canvas');
  if (!canvas) {
    throw new Error('Canvas not found in chart container');
  }

  return new Promise((resolve, reject) => {
    canvas.toBlob(async (blob) => {
      if (!blob) {
        reject(new Error('Failed to create blob from canvas'));
        return;
      }

      try {
        await navigator.clipboard.write([
          new ClipboardItem({
            'image/png': blob,
          }),
        ]);
        console.log('Chart copied to clipboard');
        resolve();
      } catch (error) {
        console.error('Failed to copy chart to clipboard:', error);
        reject(error);
      }
    }, 'image/png');
  });
}

/**
 * Helper function to download image
 */
function downloadImage(url: string, filename: string): void {
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

/**
 * Helper function to download file
 */
function downloadFile(url: string, filename: string): void {
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

/**
 * Print chart (opens print dialog with chart image)
 */
export function printChart(chart: ECharts): void {
  const url = chart.getDataURL({
    type: 'png',
    pixelRatio: 2,
    backgroundColor: '#fff',
  });

  const printWindow = window.open('', '_blank');
  if (!printWindow) {
    console.error('Failed to open print window');
    return;
  }

  printWindow.document.write(`
    <html>
      <head>
        <title>打印图表</title>
        <style>
          body {
            margin: 0;
            padding: 20px;
            display: flex;
            justify-content: center;
            align-items: center;
          }
          img {
            max-width: 100%;
            height: auto;
          }
        </style>
      </head>
      <body>
        <img src="${url}" onload="window.print(); window.close();" />
      </body>
    </html>
  `);
  printWindow.document.close();
}
