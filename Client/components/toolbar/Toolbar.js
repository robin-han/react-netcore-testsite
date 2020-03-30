import React, { Component } from 'react';
import PropTypes from 'prop-types';

import './toolbar.less';

class Toolbar extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        const {toggleSidebar, runTest, refreshTest } = this.props;
        return (
            <div className="toolbar">
                <button className="btn toggle-btn icon-show-detail" title="Toggle Sidebar" onClick={toggleSidebar}></button>
                <button className="btn run-btn" title="Run Test" onClick={runTest}></button>
                <button className="btn refresh-btn icon-refresh" title="Refresh Test" onClick={refreshTest}></button>
            </div>
        );
    }
}

Toolbar.propTypes = {
    toggleSidebar: PropTypes.func,
    runTest: PropTypes.func,
    refreshTest: PropTypes.func
};

export default Toolbar;