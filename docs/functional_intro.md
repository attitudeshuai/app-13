# FridgeWatch 功能说明文档

## 1. 业务背景与解决的问题

### 1.1 业务背景

在现代家庭生活中，冰箱是储存食材的主要场所。然而，由于工作繁忙、食材种类繁多等原因，人们常常忘记冰箱里有哪些食材，也记不清它们的保质期。这导致了两个主要问题：

1. **食物浪费** - 食材过期变质被丢弃，造成金钱和资源的浪费
2. **健康风险** - 误食过期食品可能导致肠胃不适甚至食物中毒

### 1.2 解决的问题

FridgeWatch 通过数字化管理方式，帮助用户：

- 📋 **食材清单管理** - 记录冰箱内所有食材的详细信息
- ⏰ **保质期预警** - 在食材临期/过期时主动提醒用户
- 👨‍👩‍👧 **家庭协作** - 多人共同维护冰箱，共享食材信息
- 📊 **数据洞察** - 通过统计分析了解消费习惯，优化采购计划
- 🌡️ **位置分类** - 按冷藏/冷冻/常温分类管理，更贴合实际使用场景

## 2. 用户角色与核心用例

### 2.1 用户角色

| 角色 | 说明 | 权限 |
|------|------|------|
| 访客 | 未登录用户 | 只能访问登录/注册接口 |
| 普通用户 | 注册用户 | 管理自己的信息、加入家庭 |
| 家庭成员 | 家庭中的普通成员 | 查看家庭食材、添加消耗记录 |
| 家庭所有者 | 创建家庭的用户 | 管理家庭信息、管理成员、增删改食材 |

### 2.2 核心用例

#### 用例1：用户注册登录
- 参与者：访客
- 前置条件：无
- 主流程：
  1. 用户输入用户名、邮箱、密码进行注册
  2. 系统校验信息唯一性
  3. 注册成功后自动生成 JWT Token
  4. 用户使用 Token 访问受保护接口

#### 用例2：创建家庭
- 参与者：注册用户
- 前置条件：用户已登录
- 主流程：
  1. 用户输入家庭名称
  2. 系统自动生成邀请码
  3. 创建者自动成为家庭所有者
  4. 返回家庭信息

#### 用例3：加入家庭
- 参与者：注册用户
- 前置条件：用户已登录，持有有效的邀请码
- 主流程：
  1. 用户输入邀请码
  2. 系统验证邀请码有效性
  3. 用户以成员身份加入家庭
  4. 返回成员信息

#### 用例4：添加食材
- 参与者：家庭成员
- 前置条件：用户已登录，且是家庭的成员
- 主流程：
  1. 用户选择家庭，填写食材信息（名称、分类、位置、数量、保质期等）
  2. 系统验证用户家庭成员身份
  3. 自动计算食材状态（新鲜/临期/过期）
  4. 临期食材自动创建提醒
  5. 返回食材详情

#### 用例5：记录消耗
- 参与者：家庭成员
- 前置条件：用户已登录，食材存在且有剩余数量
- 主流程：
  1. 用户选择食材，填写消耗数量
  2. 系统验证剩余数量是否充足
  3. 扣减食材数量，数量归零自动标记为已消耗
  4. 创建消耗记录
  5. 返回消耗记录详情

#### 用例6：查看统计
- 参与者：家庭成员
- 前置条件：用户已登录
- 主流程：
  1. 用户选择统计范围（家庭/个人）
  2. 系统汇总食材数量、状态分布、分类占比等数据
  3. 展示趋势图表数据
  4. 返回统计结果

## 3. 功能模块详细说明

### 3.1 用户认证模块

**功能描述**：提供用户注册、登录、个人信息管理功能，基于 JWT 实现身份认证。

**主要功能**：
- 用户注册（用户名+邮箱+密码）
- 用户登录（用户名或邮箱+密码）
- 获取当前用户信息
- 更新个人信息（用户名、邮箱、头像、密码）
- JWT Token 生成与验证

**安全机制**：
- 密码使用 PasswordHasher 哈希存储
- Token 包含用户ID、用户名、邮箱等声明
- Token 有效期 24 小时

### 3.2 家庭管理模块

**功能描述**：管理家庭/合租组的基本信息，支持多家庭管理。

**主要功能**：
- 创建家庭
- 获取家庭列表（分页、搜索）
- 获取家庭详情
- 更新家庭信息
- 删除家庭
- 邀请码自动生成

**权限规则**：
- 只有家庭所有者可以修改/删除家庭
- 成员只能查看家庭信息

### 3.3 家庭成员管理模块

**功能描述**：管理家庭中的成员，支持邀请加入、角色管理。

**主要功能**：
- 邀请码加入家庭
- 获取家庭成员列表
- 获取我的家庭列表
- 修改成员角色
- 移除成员

**权限规则**：
- 只有家庭所有者可以修改成员角色/移除成员
- 成员可以主动退出家庭（待实现）
- 不能删除家庭所有者

### 3.4 食材管理模块

**功能描述**：冰箱食材的增删改查，支持多维度筛选。

**主要功能**：
- 添加食材
- 获取食材列表（分页、搜索、排序、按家庭筛选）
- 获取食材详情
- 更新食材信息
- 删除食材
- 修改食材状态
- 自动计算保质期状态

**食材状态**：
| 状态 | 说明 | 判定规则 |
|------|------|----------|
| 新鲜(Fresh) | 食材在保质期内且距离过期较久 | 距离过期 > 3 天 |
| 临期(NearExpiry) | 即将过期，需要尽快食用 | 距离过期 ≤ 3 天且 ≥ 0 天 |
| 过期(Expired) | 已超过保质期 | 距离过期 < 0 天 |
| 已消耗(Consumed) | 已被食用完毕 | 数量为 0 或手动标记 |

**存储位置**：
- 冷藏(Fridge) - 冰箱冷藏室
- 冷冻(Freezer) - 冰箱冷冻室
- 常温(Pantry) - 常温储存

### 3.5 提醒管理模块

**功能描述**：食材临期/过期提醒，帮助用户及时处理。

**主要功能**：
- 创建提醒
- 获取提醒列表（分页、按食材筛选）
- 获取我的提醒
- 获取提醒详情
- 更新提醒
- 删除提醒
- 标记为已读
- 全部标为已读
- 未读数量统计

**提醒类型**：
- 临期提醒(NearExpiry) - 食材即将过期
- 过期提醒(Expired) - 食材已过期
- 自定义提醒(Custom) - 用户手动创建

### 3.6 消耗记录模块

**功能描述**：记录食材消耗情况，追踪使用情况。

**主要功能**：
- 记录消耗
- 获取消耗记录列表（分页、按食材/家庭筛选）
- 获取我的消耗记录
- 获取消耗详情
- 更新消耗记录
- 删除消耗记录

**业务规则**：
- 消耗数量不能超过食材剩余数量
- 消耗后自动扣减食材数量
- 食材数量为 0 时自动标记为已消耗状态

### 3.7 统计与搜索模块

**功能描述**：提供数据统计和趋势分析功能。

**主要功能**：
- 总览统计（食材总数、各状态数量、分类分布、位置分布）
- 趋势统计（按日期的添加量、消耗量、过期量趋势）
- 支持按家庭筛选

## 4. 数据库 ER 图文字描述

### 4.1 实体关系总览

```
Users (用户)
  | 1
  |
  | N
HouseholdMembers (家庭成员)
  | N
  |
  | 1
Households (家庭)
  | 1
  |
  | N
FoodItems (食材)
  | 1
  |
  | N
+-------------------+-------------------+
|                   |                   |
| N                 | N                 |
ExpiryAlerts    ConsumptionRecords   (提醒)            (消耗记录)
| N                 | N
|                   |
| 1                 | 1
Users (用户) -------+
```

### 4.2 表关系详解

| 关系 | 类型 | 说明 |
|------|------|------|
| User → HouseholdMember | 一对多 | 一个用户可以是多个家庭的成员 |
| Household → HouseholdMember | 一对多 | 一个家庭可以有多个成员 |
| Household → FoodItem | 一对多 | 一个家庭可以有多种食材 |
| FoodItem → ExpiryAlert | 一对多 | 一个食材可以有多条提醒 |
| User → ExpiryAlert | 一对多 | 一个用户可以有多条提醒 |
| FoodItem → ConsumptionRecord | 一对多 | 一个食材可以有多条消耗记录 |
| User → ConsumptionRecord | 一对多 | 一个用户可以有多条消耗记录 |

### 4.3 主要数据表说明

#### Users (用户表)
| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | int | PK, 自增 | 用户ID |
| Username | varchar(50) | UNIQUE, NOT NULL | 用户名 |
| Email | varchar(100) | UNIQUE, NOT NULL | 邮箱 |
| PasswordHash | varchar(256) | NOT NULL | 密码哈希 |
| Avatar | varchar(500) | NULL | 头像URL |
| CreatedAt | datetime | NOT NULL | 创建时间 |
| UpdatedAt | datetime | NULL | 更新时间 |

#### Households (家庭表)
| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | int | PK, 自增 | 家庭ID |
| Name | varchar(100) | NOT NULL | 家庭名称 |
| InviteCode | varchar(20) | UNIQUE, NOT NULL | 邀请码 |
| CreatedBy | int | NOT NULL | 创建者用户ID |
| CreatedAt | datetime | NOT NULL | 创建时间 |
| UpdatedAt | datetime | NULL | 更新时间 |

#### HouseholdMembers (家庭成员表)
| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | int | PK, 自增 | 记录ID |
| HouseholdId | int | FK, NOT NULL | 家庭ID |
| UserId | int | FK, NOT NULL | 用户ID |
| Role | enum | NOT NULL | 角色(Owner/Member) |
| JoinedAt | datetime | NOT NULL | 加入时间 |
| CreatedAt | datetime | NOT NULL | 创建时间 |
| UpdatedAt | datetime | NULL | 更新时间 |

**唯一约束**：HouseholdId + UserId 联合唯一

#### FoodItems (食材表)
| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | int | PK, 自增 | 食材ID |
| HouseholdId | int | FK, NOT NULL | 家庭ID |
| Name | varchar(100) | NOT NULL | 食材名称 |
| Category | varchar(50) | NOT NULL | 分类 |
| StorageLocation | enum | NOT NULL | 存储位置 |
| PurchaseDate | datetime | NOT NULL | 购买日期 |
| ExpiryDate | datetime | NOT NULL | 过期日期 |
| Quantity | decimal(18,2) | NOT NULL | 数量 |
| Unit | varchar(20) | NOT NULL | 单位 |
| PhotoUrl | varchar(500) | NULL | 图片URL |
| Status | enum | NOT NULL | 状态 |
| CreatedAt | datetime | NOT NULL | 创建时间 |
| UpdatedAt | datetime | NULL | 更新时间 |

#### ExpiryAlerts (提醒表)
| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | int | PK, 自增 | 提醒ID |
| FoodItemId | int | FK, NOT NULL | 食材ID |
| UserId | int | FK, NOT NULL | 用户ID |
| AlertType | enum | NOT NULL | 提醒类型 |
| AlertDate | datetime | NOT NULL | 提醒日期 |
| IsRead | boolean | NOT NULL, 默认false | 是否已读 |
| CreatedAt | datetime | NOT NULL | 创建时间 |
| UpdatedAt | datetime | NULL | 更新时间 |

#### ConsumptionRecords (消耗记录表)
| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | int | PK, 自增 | 记录ID |
| FoodItemId | int | FK, NOT NULL | 食材ID |
| UserId | int | FK, NOT NULL | 用户ID |
| ConsumedQuantity | decimal(18,2) | NOT NULL | 消耗数量 |
| ConsumedAt | datetime | NOT NULL | 消耗时间 |
| Note | varchar(500) | NULL | 备注 |
| CreatedAt | datetime | NOT NULL | 创建时间 |
| UpdatedAt | datetime | NULL | 更新时间 |

## 5. 关键业务规则

### 5.1 状态流转规则

**食材状态自动计算**：
- 创建/更新食材时，根据过期日期自动计算状态
- 状态判断基于当前日期与过期日期的差值
- 已消耗状态需手动设置或消耗数量为 0 时自动设置

**状态流转图**：
```
    ┌─────────┐
    │  新鲜   │
    └────┬────┘
         │ 接近保质期(≤3天)
         ▼
    ┌─────────┐
    │  临期   │
    └────┬────┘
         │ 超过保质期
         ▼
    ┌─────────┐
    │  过期   │
    └─────────┘

    ┌─────────┐
    │ 已消耗  │ ←── 手动标记或数量归零
    └─────────┘
```

### 5.2 权限规则

**家庭权限**：
- 家庭所有者(Owner)：拥有所有操作权限
- 家庭成员(Member)：查看食材、添加消耗、创建提醒
- 所有写操作（增删改）都需要验证家庭成员身份

**数据隔离**：
- 用户只能访问自己参与的家庭数据
- 列表查询默认按用户有权限的范围过滤

### 5.3 时间计算逻辑

**保质期计算**：
- 所有时间存储为 UTC 时间
- 天数计算基于日期部分，忽略具体时间
- 距离过期天数 = 过期日期 - 当前日期

**临期判断**：
- 默认临期阈值：3 天
- 可后续扩展为可配置项

### 5.4 邀请码规则

- 邀请码长度：8 位
- 字符集：大写字母 + 数字
- 全局唯一
- 创建家庭时自动生成
- 可后续扩展为可重置功能

### 5.5 消耗扣减规则

- 消耗数量必须 > 0
- 消耗数量不能超过食材剩余数量
- 消耗后自动扣减库存
- 库存归零后状态自动变为"已消耗"
- 删除消耗记录不会自动恢复库存（避免数据混乱）

## 6. 接口调用示例

### 6.1 用户注册

**请求**：
```bash
POST /api/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Test@123",
  "avatar": "https://api.dicebear.com/7.x/avataaars/svg?seed=testuser"
}
```

**响应**：
```json
{
  "code": 200,
  "message": "注册成功",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-02T00:00:00Z",
    "user": {
      "id": 6,
      "username": "testuser",
      "email": "test@example.com",
      "avatar": "https://api.dicebear.com/7.x/avataaars/svg?seed=testuser",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  }
}
```

### 6.2 添加食材

**请求**：
```bash
POST /api/fooditems
Content-Type: application/json
Authorization: Bearer <your-token>

{
  "householdId": 1,
  "name": "鲜牛奶",
  "category": "乳制品",
  "storageLocation": 1,
  "purchaseDate": "2024-01-01T00:00:00Z",
  "expiryDate": "2024-01-07T00:00:00Z",
  "quantity": 2,
  "unit": "升",
  "photoUrl": "https://example.com/milk.jpg"
}
```

**响应**：
```json
{
  "code": 200,
  "message": "创建成功",
  "data": {
    "id": 11,
    "householdId": 1,
    "name": "鲜牛奶",
    "category": "乳制品",
    "storageLocation": 1,
    "purchaseDate": "2024-01-01T00:00:00Z",
    "expiryDate": "2024-01-07T00:00:00Z",
    "quantity": 2,
    "unit": "升",
    "photoUrl": "https://example.com/milk.jpg",
    "status": 1,
    "createdAt": "2024-01-01T00:00:00Z",
    "daysToExpiry": 6
  }
}
```

### 6.3 获取统计总览

**请求**：
```bash
GET /api/stats/overview?householdId=1
Authorization: Bearer <your-token>
```

**响应**：
```json
{
  "code": 200,
  "message": "获取成功",
  "data": {
    "totalHouseholds": 1,
    "totalFoodItems": 5,
    "freshCount": 3,
    "nearExpiryCount": 1,
    "expiredCount": 1,
    "consumedCount": 0,
    "unreadAlerts": 2,
    "totalConsumedQuantity": 5.5,
    "categoryStats": [
      { "category": "乳制品", "count": 2 },
      { "category": "水果", "count": 1 },
      { "category": "蔬菜", "count": 1 },
      { "category": "肉类", "count": 1 }
    ],
    "locationStats": [
      { "location": "Fridge", "count": 3 },
      { "location": "Freezer", "count": 1 },
      { "location": "Pantry", "count": 1 }
    ]
  }
}
```
