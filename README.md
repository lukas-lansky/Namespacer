# Namespacer

In an usual REST API project, you might decide with your team that you won't call repository layer directly from controller layer -- that there will be a service layer that would reside in-between. As long as everyone remembers the rule and cares about the rule, everything is great.

The moment someone forgets or has a legitimate reason why to abandon the rule, there is nothing in a standard .NET toolset to make her and her codereviewer aware of the fact that the new code does not fit to the architecture as envisioned. .NET namespaces are just naming tools, they do not allow you to formulate visibility rules. You can try to place different things into different assemblies and then depend on properly set references, but you probably won't succeed in capturing the example presented as hypothetical controller assembly will see repository assembly transitively.

Tools like Architecture Dependency Validaton or PostSharp allow enterprise developers to enforce such namespace visibility rules. This analyzer serves the same need, but in a much simpler way -- and for free. The premise of this tool is that it's really important for every member of the team to be able to see and develop the rules. They are not here because architect tries to enforce grand vision on unwilling subjects. They are here so that people are on the same page on what should hold about the codebase right now.

## Rule definitions practically

You can define the example rule in the following positive fashion:

    Product -> Product:
      Product.Controllers -> Product.Services
      Product.Services -> Product.Repositories
      -!>
    
This says: for every mention of something from `Product` namespace in something in `Product` namespace, following must apply: it's allowed to mention services in controllers and then it's allowed to mention repositories in services, but nothing else.

This might prove to be too rigid. One day, you want to add proxy to a new network integration into `Product.Proxy.ANetworkService` namespace. You will then find out Namespacer complains: ...

One resolution would be to extend the description:

    Product -> Product:
      Product.Controllers -> Product.Services
      Product.Services -> Product.Repositories
      Product.Services -> Product.Proxy
      -!>
    
Another possibility is to redefine the rules towards explicitly forbidding what you don't want to happen -- and allowing everything else:

    Product -> Product:
      Product.Controllers -!> Product.Repositories
      ->

Please note that there is a difference between two approaches presented you might have not noticed: when you not include `Product.Services -> Product.Services`
rule, it's not possible to mention other services from service code. You are allowed to talk about the class you are in, but not about other class in the tier.
This force the layer to be very simple and pass-through, something you might want to happen in repository layer, but probably not elsewhere.

This gives the whole thing slightly fsharp-y vibe: as you surely know, in F#, order of code files in project matters as you can only mention things in
the previous files. Such rule implicitly prevents dependency cycles in the codebase.

## Rule definitions technically

`.namespacer` file might contain multiple *rules* separated by at least one empty line. Every source code *mention* must adhere to *every* rule.

Every rule starts with *scope* and ordered list of *prescriptions*. Scope says what kind of mentions are we interested in in this rule,
prescriptions are then evaluated one by one against mention we inspect.

Both scope and single prescription has either shape `namespace1 -> namespace2`, or `namespace1 -!> namespace2`. The first one allows mention of
`namespace2` in `namespace1`: when matching prescription is hit during evaluation of the rule, mention is allowed by the whole rule. The second
disallows mention of `namespace2` in `namespace1`: when matching prescription is hit, mention fails the rule and warning is reported by analyzer.

Namespace might be empty, even on both sides of the prescription. Prescription like `->` then naturally mean that every evaluation that hit this point
approves the rule.