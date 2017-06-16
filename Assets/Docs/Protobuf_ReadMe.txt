Protobuf格式协议和xml一样具有平台独立性，可以在不同平台间通信，
通信所需资源很少，并可以扩展。
Protobuf-net当然就是Protobuf在.net环境下的移植。

1.定义第一个Protocol Buffer消息
 message LogonReqMessage {
          required int64 acctID = 1;
          required string passwd = 2;
      }
 enum UserStatus {
          OFFLINE = 0;  //表示处于离线状态的用户
          ONLINE = 1;   //表示处于在线状态的用户
      }
	1. message是消息定义的关键字，等同于C++中的struct/class
	2. LogonReqMessage为消息的名字，等同于结构体名或类名。
	3. required前缀表示该字段为必要字段，既在序列化和反序列化之前该字段必须已经被赋值。
	4. int64和string分别表示长整型和字符串型的消息字段
	5. acctID和passwd分别表示消息字段名，等同于C++中的成员变量名。
	6. 标签数字1和2则表示不同的字段在序列化后的二进制数据中的布局位置。布局位置与变量声明顺序无关
		备注：需要注意的是该值在同一message中不能重复， 另外，对于Protocol Buffer而言，标签值为1到15的字段在编码时可以得到优化，
		既标签值和类型信息仅占有一个byte，标签范围是16到2047的将占有两个bytes，而Protocol Buffer可以支持的字段数量则为2的29次方减一。
		有鉴于此，我们在设计消息结构时，可以尽可能考虑让repeated类型的字段标签位于1到15之间，这样便可以有效的节省编码后的字节数量。
	7.可以使用"//"添加注释,等同于C++
	8.enum枚举关键字，枚举值指定任意整型值，而无需总是从0开始定义，等同于C++
	

2.同一个.proto文件中定义多个message，消息可以嵌套

3.其他消息定义文件可以通过import的方式将该文件中定义的消息包含进来

4.限定符(required/optional/repeated)的基本规则
	1. 在每个消息中必须至少留有一个required类型的字段。 
	2. 每个消息中可以包含0个或多个optional类型的字段。
	3. repeated表示的字段可以包含0个或多个数据。
	4. 如果打算在原有消息协议中添加新的字段，同时还要保证老版本的程序能够正常读取或写入，
		那么对于新添加的字段必须是optional或repeated。道理非常简单，老版本程序无法读取或写入新增的required限定符的字段。

5.Protocol Buffer消息升级原则
	1. 不要修改已经存在字段的标签号。
	2. 任何新添加的字段必须是optional和repeated限定符，否则无法保证新老程序在互相传递消息时的消息兼容性。
	3. 在原有的消息中，不能移除已经存在的required字段，optional和repeated类型的字段可以被移除，但是他们之前使用的标签号必须被保留，不能被新的字段重用。
	4. int32、uint32、int64、uint64和bool等类型之间是兼容的，sint32和sint64是兼容的，string和bytes是兼容的，fixed32和sfixed32，以及fixed64和sfixed64之间是兼容的，这意味着如果想修改原有字段的类型时，为了保证兼容性，只能将其修改为与其原有类型兼容的类型，否则就将打破新老消息格式的兼容性。
	5. optional和repeated限定符也是相互兼容的。

6.Packages对应C++命名空间或java中的包名