const TestResultState = Object.freeze({
    NotRun: 0,
    Success: 1,
    Failure: 2,
    Error: 3
});

const TestState = Object.freeze({
    Unstarted: 0,
    Running: 1,
    Suspended: 2,
    Stopped: 3
});

export { TestResultState, TestState }