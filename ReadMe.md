Typed, Embedded Datalog
================

This is a predagogical implementation of Datalog for use in GameAI.  It does not yet implement
recursive rules, but it supports strong typing and goes to considerable lengths to minimize garbage
collection and run-time type-checking.

It's basically the same as TELL, however, being a Datalog derivative, it computes the full extensions
of predicates, and does so bottom-up.  Each non-primitive predicate holds a table of all the ground
instances of the prediate (its "rows").  So if you define a predicate p interms of a predicate q,
and define q in terms of r, then you would manually load data into r using AddRow().  Then it would
compute a table of all the rows of q from r, then compute a table of all the rows p from q.

While this sounds crazy, it makes sense for databases and certain kinds of game applications,
or so we hope.  And in cases where you really do want to compute complete extensions of things, it
has a lot of advantages over the kind of top-down evaluation used in Prolog-style languages like TELL:
* You don't need to create fresh variables for a rule each time it is called
* Variables can never be aliased to oneanother because unification only occurs between variables and 
  ground instances
* Consequently, no binding lists are necessary; space for variables in rules can be statically allocated
* Better cache locality because of the use of table data structures
* You effectively do common-subexpression elimination by tabling all the data
* Tables that don't depend on one another can be computed in parallel (not yet implemented)
* Mode analysis of variables can always be statically determined

Stuff to do:
* Implement recursion
* Implement parallel evaluation
* Implement indexing (this is probablym most important)