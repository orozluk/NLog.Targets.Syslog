Syslog target for NLog
======================
**NLog Syslog** is a custom target for **NLog**: [http://nlog-project.org](http://nlog-project.org/).
**This is a fork of an original work of @luigiberrettini** [https://github.com/luigiberrettini/NLog.Targets.Syslog](https://github.com/luigiberrettini/NLog.Targets.Syslog)

I have forked this project to introduce two modifications:

1. The possibility to define your own Server Certificate Validation Callback
2. The closure of the socket after sending each message.

