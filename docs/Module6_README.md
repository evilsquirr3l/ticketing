# Caching and Multithreading

The purpose of the module is to consolidate in practice the materials on ‘Multithreading’ topics with a focus on data
access synchronization options on the example of caching logic implementation in a multithreaded ASP.NET API. In
addition, you will learn about cache invalidation strategies.

### What should be done

- Task 1:
  Implement caching logic for Event resource. It can be either In-Memory or Distributed by your choice (discuss with
  mentor if you have hesitations). Modify POST/PUT endpoints of Order resource to invalidate Event cache. Leave DELETE
  endpoints untouched for testing needs.

- Task 2:
  In addition to task 1 cover ‘Event’ resource endpoints (/events/*) using request (HTTP) caching allow client
  applications perform cache management on their side and avoid making additional requests to the server with no need.
