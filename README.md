# FridgeWatch - 家庭冰箱食材保质期预警系统

## 项目简介

FridgeWatch 是一款帮助家庭/合租室友管理冰箱内食材的后端系统。记录购买日期、保质期与存放位置，在临期或过期前自动提醒，减少食物浪费并避免误食过期食品。

## 功能亮点

- 🥬 **食材位置可视化** - 支持冷藏、冷冻、常温三种存储位置分类管理
- ⏰ **智能保质期预警** - 临期自动提醒，避免食物浪费
- 👨‍👩‍👧 **家庭/合租共享** - 邀请码加入，多人共享冰箱
- 📊 **消耗统计分析** - 记录食材消耗，掌握家庭饮食规律
- 🔐 **JWT 安全认证** - 标准 Token 鉴权，保障数据安全

## 技术栈

| 类别 | 技术 |
|------|------|
| 后端框架 | .NET Core 8.0 (ASP.NET Core Web API) |
| 数据库 | MySQL 8.0 |
| ORM | Entity Framework Core 8.0 (Pomelo.EntityFrameworkCore.MySql) |
| 认证 | JWT Bearer Token |
| API文档 | Swagger / OpenAPI |
| 容器化 | Docker + Docker Compose |
| 测试 | xUnit |
| 对象映射 | AutoMapper |
| 参数校验 | FluentValidation |
| 日志 | Serilog |

## 目录结构

```
FridgeWatch/
├── src/
│   ├── FridgeWatch.API/          # API层 - 控制器、中间件
│   │   ├── Controllers/          # API控制器
│   │   ├── Middleware/           # 中间件
│   │   └── Extensions/           # 扩展方法
│   ├── FridgeWatch.Application/  # 应用层 - 业务逻辑
│   │   ├── DTOs/                 # 数据传输对象
│   │   ├── Services/             # 服务实现
│   │   ├── Interfaces/           # 服务接口
│   │   ├── Mappings/             # AutoMapper映射
│   │   └── Validators/           # FluentValidation验证器
│   ├── FridgeWatch.Domain/       # 领域层 - 实体与接口
│   │   ├── Entities/             # 实体模型
│   │   ├── Enums/                # 枚举
│   │   ├── Common/               # 公共基类
│   │   └── Interfaces/           # Repository接口
│   └── FridgeWatch.Infrastructure/ # 基础设施层
│       ├── Data/                 # DbContext、Seed Data
│       ├── Repositories/         # Repository实现
│       └── Services/             # 基础设施服务
├── tests/
│   └── FridgeWatch.Tests/        # 单元测试/集成测试
├── docs/
│   └── functional_intro.md       # 功能说明文档
├── Dockerfile                    # Docker镜像构建文件
├── docker-compose.yml            # Docker Compose配置
└── README.md                     # 项目说明
```

## 快速启动

### 前置要求

- Docker 20.10+
- Docker Compose v2+

### 启动服务

```bash
# 克隆并进入项目目录
git clone <repo-url>
cd FridgeWatch

# Docker一键启动
docker-compose up --build -d
```

### 查看日志

```bash
# 查看应用日志
docker-compose logs -f app

# 查看MySQL日志
docker-compose logs -f mysql
```

### 验证服务

```bash
# 健康检查
curl http://localhost:8083/health

# 返回示例: {"status":"Healthy","timestamp":"..."}
```

### 访问接口文档

启动后访问: [http://localhost:8083](http://localhost:8083) 即可打开 Swagger UI

### 数据库管理

Adminer 数据库管理工具: [http://localhost:8080](http://localhost:8080)
- 服务器: `mysql`
- 用户名: `app_user`
- 密码: `app_pass`
- 数据库: `fridgewatch`

### 停止服务

```bash
docker-compose down -v
```

## API 接口概览

### 认证模块
- `POST /api/auth/register` - 用户注册
- `POST /api/auth/login` - 用户登录
- `GET /api/auth/me` - 获取当前用户信息
- `PUT /api/auth/me` - 更新个人信息

### 家庭管理
- `GET /api/households` - 获取家庭列表
- `POST /api/households` - 创建家庭
- `GET /api/households/{id}` - 获取家庭详情
- `PUT /api/households/{id}` - 更新家庭
- `DELETE /api/households/{id}` - 删除家庭

### 家庭成员
- `GET /api/householdmembers` - 获取成员列表
- `POST /api/householdmembers/join` - 加入家庭
- `GET /api/householdmembers/mine` - 我的家庭
- `GET /api/householdmembers/{id}` - 获取成员详情
- `PUT /api/householdmembers/{id}` - 更新成员角色
- `DELETE /api/householdmembers/{id}` - 移除成员

### 食材管理
- `GET /api/fooditems` - 获取食材列表
- `POST /api/fooditems` - 添加食材
- `GET /api/fooditems/{id}` - 获取食材详情
- `PUT /api/fooditems/{id}` - 更新食材
- `DELETE /api/fooditems/{id}` - 删除食材
- `PATCH /api/fooditems/{id}/status` - 修改食材状态

### 提醒管理
- `GET /api/expiryalerts` - 获取提醒列表
- `POST /api/expiryalerts` - 创建提醒
- `GET /api/expiryalerts/mine` - 我的提醒
- `GET /api/expiryalerts/{id}` - 获取提醒详情
- `PUT /api/expiryalerts/{id}` - 更新提醒
- `DELETE /api/expiryalerts/{id}` - 删除提醒
- `GET /api/expiryalerts/unread-count` - 未读提醒数量

### 消耗记录
- `GET /api/consumptionrecords` - 获取消耗记录
- `POST /api/consumptionrecords` - 记录消耗
- `GET /api/consumptionrecords/mine` - 我的消耗记录
- `GET /api/consumptionrecords/{id}` - 获取消耗详情
- `PUT /api/consumptionrecords/{id}` - 更新消耗记录
- `DELETE /api/consumptionrecords/{id}` - 删除消耗记录

### 统计分析
- `GET /api/stats/overview` - 总览统计
- `GET /api/stats/trend` - 趋势统计

## 测试

### Postman 测试

项目根目录提供 `postman_collection.json`，导入 Postman 后可直接测试所有接口。

### 自动化测试

```bash
# 执行单元测试
dotnet test
```

## 初始测试账号

系统启动后自动创建以下测试账号:

| 用户名 | 密码 | 邮箱 |
|--------|------|------|
| admin | Admin@123 | admin@example.com |
| alice | Alice@123 | alice@example.com |
| bob | Bob@123 | bob@example.com |
| charlie | Charlie@123 | charlie@example.com |
| diana | Diana@123 | diana@example.com |

## 统一返回格式

所有接口均返回统一格式:

```json
{
  "code": 200,
  "message": "操作成功",
  "data": { ... }
}
```

## 贡献指南

欢迎提交 Issue 和 Pull Request！

## 许可证

MIT License
