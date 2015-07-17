TaskSetup {
  "Executing task setup"
}

TaskTearDown {
  "Executing task tear down"
}

Task default -depends TaskB

Task TaskA {
  "TaskA executed"
}

Task TaskB -depends TaskA {
  "TaskB executed"
}
