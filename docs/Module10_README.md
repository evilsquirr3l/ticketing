# REST API Load Testing & Profiling

The purpose of the module is to consolidate in practice the materials on ‘Analyzing and profiling tools’ module, review
load testing theory and tools. The first practical tasks will include analyses of the performance metrics of the API
under different load circumstances. The second task will require you to perform a profiling of a specific case to
determine a possible bottleneck.

### What should be done

- Task 1
  Fill the table with the load test result values for ‘/events/{event_id}/sections/{section_id}/seats’ endpoint on your
  deployed application. Consider 3-5 options for event_id and 2-3 section_id per each event (all ids should exist and
  database), make sections contain at least 50 seats.
    - You can either use self-written tool to get these numbers (HttpClient, Stopwatch, Task, Task.WaitAll) or use any
      existing tool like bombardier.
        - You can try to repeat this applying different scaling options (vertical, horizontal)


- Task 1 (Alternative):
  Instead of filling a table you can use specialized tools to create charts on each parameter to see how concurrent
  threads number impacts parameters in dynamic.

- Task 2
  Profile ‘POST orders/carts/{cart_id}’ endpoint and determine the bottleneck place in its flow. Check it cases when
  optimistic and pessimistic strategies are used. Compare.
