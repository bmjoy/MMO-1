
 # 多人在线的动作游戏 
 ## 技术特点:
  * 1.使用unity为服务器。<br/>
  * 2.协议使用protobuf自动生产<br/>
  * 3.UI系统使用自动生产代码<br/>
  * 4.使用mongodb作为数据储存<br/>
  * 5.使用AI行为树的AI逻辑处理<br/>
  * 6.技能编辑可视化<br/>
  * 7.基于状态同步的技术<br/>

## 编辑效果查看：<br/>
https://www.youtube.com/watch?v=xxkcTMaQHDs&t=190s<br/>

  
  ## 项目运行需求
  *  mongodb 版本
  *  Unity发布的server版本
  
  ## 项目目录结构
  *  client 战斗服务器和游戏客户端
  *  Doc 是相关策划文档已经迁移到了 [项目相关文档](https://drive.google.com/drive/folders/1yl8qRea4k8GfgQMEJbQq0JGRSVvwrRnv?usp=sharing)
  *  Server 服务器目录
  *  Server/GServer/LoginServer 是中心账号服务器
  *  Server/GServer/GServer 是网关服务器用来承载用户数据
  *  PublicTools 相关的工具目录
  *  PublicTools/econfigs 游戏的数据配表
  *  PublicTools/proto 游戏的网络协议
  *  PublicTools/src 自动编译工具源码输出目录
  *  PublicTools/toolssrc 工具的源码
  
  
  ## 项目启动
  *  启动服务器 运行脚本 start_test_servers.sh 
  *  发布unity客户端 和 服务器配置相关服务器参数
  *  Unity配置在 client/Assets/StreamingAsset 目录下可以配置战斗服务器和客户端的链接信息
  
  
  
  
