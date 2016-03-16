OrderProcessingSystem

This is an experimental project for practising developing a distributed order process system with production bar including: functionality, coding style, corner cases coverage, unit test, performance tunning, operationing. Following are the requirement:


Develop an Order processing System
==================================
1.  Customer can place an order and submit to this system,   system will return customer an uniq order-id and async process that order. 
2.  Each Order go through 4 steps (Scheduling, Pre-Processing,  Processing,  Post-Processing ). Each Step need 5 seconds for processing.  There is 2 extra steps (Completed, Failed) indicating Failure.  
3.  Customer can use order ID to query order details.  Order detail at least include:  Order ID,  current step,  Order start time and complete time,  each step¡¯s start time and complete time.   
4.  If there¡¯s any failure among above 5 steps,  system need to rollback and mark order as failure.  Each Step has 5% of failure ratio. 
5.  Order Processing System is a distributed cluster with N nodes,  each node have 50 working threads.   System can sustain more orders by simply adding more nodes.  System should not degrading when more and more nodes are added into cluster. 
6.  If any of node is down,  other node in cluster will pick that order up and continue processing.   There will not be more than 1 node to pick up any given order at the same time. 
7.  System should be easy for trouble shooting.  Corresponding debugging and operation interface need to be considered when this order system is in production and hand over to operation. 

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


