/**
 * Performance Test Configuration using k6
 * 使用 k6 的性能测试配置
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const apiResponseTime = new Trend('api_response_time');

// Test configuration
export const options = {
  stages: [
    { duration: '2m', target: 10 },   // Ramp up to 10 users
    { duration: '5m', target: 10 },   // Stay at 10 users
    { duration: '2m', target: 50 },   // Ramp up to 50 users
    { duration: '5m', target: 50 },   // Stay at 50 users
    { duration: '2m', target: 100 },  // Ramp up to 100 users
    { duration: '5m', target: 100 },  // Stay at 100 users
    { duration: '5m', target: 0 },    // Ramp down to 0 users
  ],
  thresholds: {
    'http_req_duration': ['p(95)<500', 'p(99)<1000'], // 95% < 500ms, 99% < 1s
    'http_req_failed': ['rate<0.01'],                  // Error rate < 1%
    'errors': ['rate<0.1'],                            // Custom error rate < 10%
  },
};

const BASE_URL__ENV__.BASE_URL || 'http://localhost:5000';

// Login helper
function login() {
  const loginPayload = JSON.stringify({
    username: 'testuser',
    password: 'testpass123',
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const loginRes = http.post(`${BASE_URL}/api/v1/auth/login`, loginPayload, params);

  check(loginRes, {
    'login successful': (r) => r.status === 200,
  });

  try {
    const responseBody = JSON.parse(loginRes.body);
    return responseBody.accessToken;
  } catch (e) {
    console.error('Failed to parse login response');
    return null;
  }
}

export default function () {
  // Login
  const token = login();
  if (!token) {
    errorRate.add(1);
    return;
  }

  const authHeaders = {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  };

  // Test 1: Get market summary
  const marketRes = http.get(`${BASE_URL}/api/v1/market/summary`, authHeaders);
  check(marketRes, {
    'market summary status 200': (r) => r.status === 200,
    'market summary response time < 500ms': (r) => r.timings.duration < 500,
  });
  apiResponseTime.add(marketRes.timings.duration);

  sleep(1);

  // Test 2: Get orders
  const ordersRes = http.get(`${BASE_URL}/api/v1/orders`, authHeaders);
  check(ordersRes, {
    'orders status 200': (r) => r.status === 200,
    'orders response time < 500ms': (r) => r.timings.duration < 500,
  });
  apiResponseTime.add(ordersRes.timings.duration);

  sleep(1);

  // Test 3: Get positions
  const positionsRes = http.get(`${BASE_URL}/api/v1/positions`, authHeaders);
  check(positionsRes, {
    'positions status 200': (r) => r.status === 200,
    'positions response time < 500ms': (r) => r.timings.duration < 500,
  });
  apiResponseTime.add(positionsRes.timings.duration);

  sleep(1);

  // Test 4: Get strategies
  const strategiesRes = http.get(`${BASE_URL}/api/v1/strategies`, authHeaders);
  check(strategiesRes, {
    'strategies status 200': (r) => r.status === 200,
    'strategies response time < 500ms': (r) => r.timings.duration < 500,
  });
  apiResponseTime.add(strategiesRes.timings.duration);

  sleep(1);

  // Test 5: Create order (write operation)
  const orderPayload = JSON.stringify({
    symbol: 'AAPL',
    quantity: 10,
    side: 'Buy',
    type: 'Market',
  });

  const createOrderRes = http.post(
    `${BASE_URL}/api/v1/orders`,
    orderPayload,
    authHeaders
  );

  check(createOrderRes, {
    'create order status 200 or 201': (r) => r.status === 200 || r.status === 201,
    'create order response time < 1000ms': (r) => r.timings.duration < 1000,
  });
  apiResponseTime.add(createOrderRes.timings.duration);

  sleep(2);
}

// Teardown
export function teardown(data) {
  console.log('Performance test completed');
}
