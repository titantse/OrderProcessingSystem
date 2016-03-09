
OrderProcessingSystem
======================

Platform: C# + SQL Server

======================
Design
In this system, I use SQL Server as a queue for async processing, cos with DB we could have some benefit to isolate the tasks and fault toerant.
WorkNode is the processing program basically including two parts. One for preodically pull data from DB with 1 thread(called scheduler), and the other are multiple threads to process the order(called workers).

In WorkNode, there is an in-memory queue to help to implement pub-sub pattern. and another thread which is responsible to report health to DB, so that when an WorkNode down, DB would know and assign new orders to other living nodes.

The pons of this design is simple, and code logic is clear, but the cons is that we added DB's burden, for the next phase, we could introduce cash to reduce the DB access.

The coding and test of the system is completed. The performance improvement hasn't been done yet.


Deployment
=============================

1.Database.
2.Deploy ElasticSearch and Kibana.
3.Deploy Service.(OderProcessing.WebAPI)
4.Deploy WorkNode.


Ops
============================
When WorkNode is started, following url are help to monitor the if the node works well.
http://localhost:8765/comm-and/stat Statistics of the node.
http://localhost:8765/command/stop Stop the node.
http://localhost:8765/Watchdog_Selfclosure Watchdog self closure.
http://localhost:8765/Watchdog_External Watchdog check external dependy(DB)


Benchmark
============================


