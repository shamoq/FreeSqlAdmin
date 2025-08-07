👏**.NET 8来了，开发一套基于.NET 8的快速开发框架吧，就叫** Simple [点我✨Star](https://gitee.com/VCodeWork/simple-framework)  

# 👀 框架由来

为啥有了那么多开发框架，还得非要再搞个呢，大家都知道很多快速开发框架，啥ABP 啥水果啥的，怎么评价他们呢，就是一个字，**叼！**。
可是作为一个打工人，还不是公司用啥我用啥，公司不用ABP，你也不能老新建个项目就上ABP,上各种Orm吧，并且用ABP类的框架，还是需要些学习时间的，这些框架各种约定，各种快捷Api，各种一键生成了啥啥啥，好用吧，可是一但你公司不能用这些框架，你是不是抓瞎了，之前遇到个只用ABP的新同事，进公司要求只能用基于ASP.Net Core的框架和指定的一些类库，要他开发个文件上传的模块，用于客户服务器打开网页上传下数据库备份之类的文件到公司备份服务器做异地备份，硬是边看文档边搞了，干了2个周，说白了已经不知道ASP.Net Core是咋玩的了。

# 🎈 框架特点

  由此我萌发了写个快速开发的框架，他需要有这些特点：

1. 开发的框架基于 .NET 8 各种特性啥的尽量少,但是尽量方便点，比如实现自动注入

2. 除非很需要，框架本身尽量少引用第三方库，但是又要方便后期添加需要用到的第三方库

3. 需要有基本的授权、鉴权，免得每次都要去写这些通用的东西

4. 需要实现一套 CRUD 实现，让大量curd解放，能快速生成，同时能方便使用代码生成器生成这一套东西

5. 需要实现一套后台管理的前端，并实现基础页面，如登录、系统管理相关页面

6. 添加一些工具，用于帮助部署或者生成项目
   
   # ❤ 规划
   
   根据以上构想，做了一些规划：
   
   1. 使用 NLog 做日志库
   2. 使用 Redis 库 StackExchange.Redis
   3. 使用 Json 库 Newtonsoft.Json
   4. 使用 LitDb 轻量数据库做应用存储库
   5. 使用 MediatR 做应用内事件分发库
   6. 使用 FreeSql Core做默认 Orm
   7. 使用 FluentScheduler 做定时任务模块
   8. 使用多Controller项目模块，支持让各自的api模块使用独立项目，比如AdminController使用一个项目，其他的又分别可以使用自己的项目
   9. 支持多数据库结构，可以让不同的业务范围使用不同的数据库，比如系统管理模块使用MSSQL DB1,业务模块 使用Mysql DB2
   10. 使用winSW 搭建一个建议启动项目为服务的模块
   11. 还有最重要的鉴权模块，使用jwt处理鉴权标识的问题，后端实现一套功能点的体系，可以为任何一个你想进行权限控制的api进行权限控制，并可任意搭配分配给你的角色或者人员

![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/1.png)

# 🕐话不多说，上图吧

## 添加基础库 Simple.Core 添加基础库 Simple.Core

基础库主要包含一些帮助类，用于方便开发的，和其他框架大同小异
![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/2.png)

## Web项目扩展库 Simple.Core.Web

![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/3.png)

主要实现： 
💛 应用模块基类 SimpleModule，继承此模块基类可分模块开发，分模块按需注册、配置管道，配置模块初始化
💛 自动注入，自动扫描标注了自动注入的任何类和实现，不包含一对多实现
💛 鉴权基类控制器 AppAuthController
💛 Curd控制器 AppCurdController 
💛 应用事件分发管理 AppDomainEventDispatcher
💛 简单主机 SimpleHost ，该类用于一键启动按默认配置好服务的 web主机、控制台主机
💛 其他web类，jwt相关，异常和鉴权相关
💛 主机扩展类 HostServiceExtension 里面包含各种服务注册、获取，模块注册，权限自动生成的方法

## 定时任务 Simple.Job

基于 FluentScheduler 封装的自动注册任务，实现任务自动注册，配置基类用于可使每个任务能单独配置执行调度器

![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/6.png)

## 🎃 **以上就是整个框架的封装，够简单吧，是不是感觉啥都没有用，就这么点东西已经完成了我上面构想的支撑！**

下面大家看下admin模块和common模块，分别都能实现自动注入、定时任务、自动生成各自的数据库及数据初始化、领域事件处理，顺带说下，不管是控制台应用的主机还是webapi的主机，里面的模块都可使用构造函数注入哦

![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/7.png)

看admin控制器项目及主机启动，后面加业务控制器啥的，直接新建项目就行，控制台主机也自动支持了自动注入啥的，开发服务啊啥的都可以重复使用上面Applications下的任意实现

WebHost 主机项目
![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/8.png)

启动 WebHost 主机

![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/9.png)

看控制台项目

![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/10.png)

启动控制台主机

![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/11.png)


## 适配了一套基于vue3 navieui 前端页面
悄悄说下，还适配了一套基于vue3 navieui,已完成登录，菜单、角色、角色授权、用于管理哦
![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/14.png)


## 注意配置文件的开发和发布隔离
对了，注意配置文件的开发和发布隔离哦

![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/12.png)

![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/13.png)

## 新增AOT项目 快速部署服务
新增Simple.RunService项目，快速部署服务，很好使用，AOT编译，不用安装环境 X64可用。

![image](https://github.com/shamoq/FreeSqlAdmin/blob/main/src/cutImgs/15.png)

好了，用最简单的东西实现了大多数项目业务都能搞定的框架了吧，如果对你有用，记得 [点我✨Star](https://gitee.com/VCodeWork/simple-framework) 哦，初步版本已提交，有时间会持续优化，代码生成器还有一丢丢没搞玩，其他都差不多了，需要的看代码去吧。

--------
以上内容，引用 [源仓库](https://gitee.com/VCodeWork/simple-framework) ，做了少量修改。

# 基于源框架主要修改点
1. 增加vben5作为前端
2. 切换成FreeSql作为Orm，支持各大数据库
3. 增加多租户模式，基于FreeSqlCloud支持共享数据库和独立数据库双模式


# 后续计划
1. 增加通用参数生成
2. 增加操作权限控制
3. 增加操作日志