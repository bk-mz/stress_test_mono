Stress test utility app for MONO
================

This utility is created to test mono installations. Goal is to check whether mono is suitable in high-load production environments.

Utility constantly spawns worker threads to to some cpu-bound work. Some IO work (related to file reads) is also introduced.

Worker tasks math tasks were taken from http://benchmarksgame.alioth.debian.org.

Every worker class is loaded in separate domain.

Exceptions are thrown, too.

= Usage =

`make` to run mono-boehm.

OR

`make run-sgen` to run mono with sgen gc.


