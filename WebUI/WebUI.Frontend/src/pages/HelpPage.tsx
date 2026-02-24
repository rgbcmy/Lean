/**
 * Help Center Page Component
 * 帮助中心页面组件
 */

import React from 'react';
import { Typography, Card, Row, Col, Collapse, Space, Button } from 'antd';
import {
  QuestionCircleOutlined,
  BookOutlined,
  CustomerServiceOutlined,
  GithubOutlined,
  FileTextOutlined,
  ApiOutlined,
} from '@ant-design/icons';
import './HelpPage.css';

const { Title, Paragraph, Text } = Typography;
const { Panel } = Collapse;

/**
 * Help Center Page Component
 * 帮助中心页面
 */
const HelpPage: React.FC = () => {
  const quickLinks = [
    {
      key: 'user-guide',
      icon: <BookOutlined />,
      title: '用户指南',
      description: '查看完整的用户使用手册',
      link: '/docs/user-guide',
    },
    {
      key: 'api-docs',
      icon: <ApiOutlined />,
      title: 'API 文档',
      description: '浏览 API 接口文档',
      link: '/docs/api',
    },
    {
      key: 'changelog',
      icon: <FileTextOutlined />,
      title: '更新日志',
      description: '查看版本更新历史',
      link: '/docs/changelog',
    },
    {
      key: 'github',
      icon: <GithubOutlined />,
      title: 'GitHub',
      description: '访问项目源码仓库',
      link: 'https://github.com/QuantConnect/Lean',
      external: true,
    },
  ];

  const faqs = [
    {
      question: '如何连接 IBKR 账户？',
      answer: (
        <>
          <Paragraph>
            1. 确保 TWS (Trader Workstation) 或 IB Gateway 已启动并登录
          </Paragraph>
          <Paragraph>
            2. 在 TWS/Gateway 中启用 API 连接（配置 → API → 设置）
          </Paragraph>
          <Paragraph>
            3. 在本系统的"设置 → IBKR 配置"页面填写连接参数
          </Paragraph>
          <Paragraph>
            4. 点击"测试连接"按钮验证连接是否成功
          </Paragraph>
        </>
      ),
    },
    {
      question: '如何下单交易？',
      answer: (
        <>
          <Paragraph>
            1. 前往"交易 → 股票交易"页面
          </Paragraph>
          <Paragraph>
            2. 搜索您要交易的股票代码
          </Paragraph>
          <Paragraph>
            3. 选择订单类型（市价单或限价单）
          </Paragraph>
          <Paragraph>
            4. 输入交易数量和价格（限价单）
          </Paragraph>
          <Paragraph>
            5. 点击"提交订单"确认交易
          </Paragraph>
        </>
      ),
    },
    {
      question: '如何创建和运行策略？',
      answer: (
        <>
          <Paragraph>
            1. 前往"策略 → 新建策略"页面
          </Paragraph>
          <Paragraph>
            2. 选择策略模板或上传自己的策略代码
          </Paragraph>
          <Paragraph>
            3. 配置策略参数（初始资金、风控规则等）
          </Paragraph>
          <Paragraph>
            4. 保存策略后，在策略详情页点击"启动"按钮
          </Paragraph>
          <Paragraph>
            5. 实时查看策略运行日志和订单执行情况
          </Paragraph>
        </>
      ),
    },
    {
      question: '如何进行回测？',
      answer: (
        <>
          <Paragraph>
            1. 前往"回测 → 新建回测"页面
          </Paragraph>
          <Paragraph>
            2. 选择要回测的策略
          </Paragraph>
          <Paragraph>
            3. 设置回测时间范围和初始资金
          </Paragraph>
          <Paragraph>
            4. 点击"开始回测"，系统会调用 Lean 引擎执行回测
          </Paragraph>
          <Paragraph>
            5. 回测完成后查看收益曲线、夏普比率、最大回撤等指标
          </Paragraph>
        </>
      ),
    },
    {
      question: '支持哪些市场和产品？',
      answer: (
        <>
          <Paragraph>
            当前版本支持：
          </Paragraph>
          <ul>
            <li>美股股票（通过 IBKR）</li>
            <li>美股 ETF（通过 IBKR）</li>
          </ul>
          <Paragraph>
            未来版本将支持期货、期权、外汇等更多产品类型。
          </Paragraph>
        </>
      ),
    },
    {
      question: '如何设置风险控制？',
      answer: (
        <>
          <Paragraph>
            前往"风控 → 风控配置"页面，可以设置：
          </Paragraph>
          <ul>
            <li>止损/止盈规则</li>
            <li>单笔订单最大金额</li>
            <li>单日交易次数限制</li>
            <li>持仓集中度限制</li>
            <li>最大回撤告警阈值</li>
          </ul>
          <Paragraph>
            系统会实时监控并强制执行这些风控规则。
          </Paragraph>
        </>
      ),
    },
    {
      question: '数据如何存储？',
      answer: (
        <>
          <Paragraph>
            系统支持两种数据库：
          </Paragraph>
          <ul>
            <li><strong>PostgreSQL</strong>：推荐用于生产环境，性能和稳定性更好</li>
            <li><strong>SQLite</strong>：适合开发和测试，无需额外安装数据库</li>
          </ul>
          <Paragraph>
            可以在"设置 → 系统配置"页面切换数据库类型。
          </Paragraph>
        </>
      ),
    },
    {
      question: '遇到问题如何获取帮助？',
      answer: (
        <>
          <Paragraph>
            您可以通过以下方式获取帮助：
          </Paragraph>
          <ul>
            <li>查看用户指南和 API 文档</li>
            <li>在 GitHub Issues 提交问题</li>
            <li>加入 Lean 社区 Discord 频道</li>
            <li>发送邮件至技术支持邮箱</li>
          </ul>
        </>
      ),
    },
  ];

  return (
    <div className="help-page">
      {/* Header */}
      <div className="help-header">
        <Title level={2}>
          <QuestionCircleOutlined /> 帮助中心
        </Title>
        <Text type="secondary">
          查找使用指南、常见问题解答和技术文档
        </Text>
      </div>

      {/* Quick Links */}
      <Card title="快速链接" bordered={false} className="quick-links-card">
        <Row gutter={[16, 16]}>
          {quickLinks.map((link) => (
            <Col xs={24} sm={12} lg={6} key={link.key}>
              <Card
                hoverable
                className="quick-link-card"
                onClick={() => {
                  if (link.external) {
                    window.open(link.link, '_blank');
                  } else {
                    window.location.href = link.link;
                  }
                }}
              >
                <Space direction="vertical" align="center" style={{ width: '100%' }}>
                  <div className="quick-link-icon">{link.icon}</div>
                  <Text strong>{link.title}</Text>
                  <Text type="secondary" style={{ textAlign: 'center', fontSize: 12 }}>
                    {link.description}
                  </Text>
                </Space>
              </Card>
            </Col>
          ))}
        </Row>
      </Card>

      {/* FAQs */}
      <Card
        title="常见问题"
        bordered={false}
        className="faqs-card"
        style={{ marginTop: 24 }}
      >
        <Collapse
          accordion
          bordered={false}
          expandIconPosition="end"
          className="faqs-collapse"
        >
          {faqs.map((faq, index) => (
            <Panel
              header={<Text strong>{faq.question}</Text>}
              key={index.toString()}
            >
              <div className="faq-answer">{faq.answer}</div>
            </Panel>
          ))}
        </Collapse>
      </Card>

      {/* Contact Support */}
      <Card
        title="联系支持"
        bordered={false}
        className="contact-card"
        style={{ marginTop: 24 }}
      >
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          <Paragraph>
            如果以上信息无法解决您的问题，欢迎通过以下方式联系我们：
          </Paragraph>
          <Space wrap>
            <Button
              icon={<GithubOutlined />}
              onClick={() => window.open('https://github.com/QuantConnect/Lean/issues', '_blank')}
            >
              GitHub Issues
            </Button>
            <Button
              icon={<CustomerServiceOutlined />}
              onClick={() => window.open('https://discord.gg/lean', '_blank')}
            >
              Discord 社区
            </Button>
            <Button
              icon={<BookOutlined />}
              onClick={() => window.open('https://www.quantconnect.com/docs', '_blank')}
            >
              官方文档
            </Button>
          </Space>
        </Space>
      </Card>
    </div>
  );
};

export default HelpPage;
