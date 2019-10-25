import React, { Component } from 'react';
import PropTypes from 'prop-types';

import './result-view.less';

class ResultView extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        const { title, result, expected, message } = this.props;
        if (message) {
            console.log(message);
        }

        return (
            <div className="result-view">
                <div className="title" title={title}>
                    <span>{title}</span>
                </div>
                <div className="result">
                    <div className="current"
                        dangerouslySetInnerHTML={{ __html: result }}>
                    </div>
                    <div className="expected"
                        dangerouslySetInnerHTML={{ __html: expected }}>
                    </div>
                </div>
                {/* <textarea className="message" value={message}></textarea> */}
            </div>
        );
    }
}

ResultView.propTypes = {
    title: PropTypes.string,
    result: PropTypes.string,
    expected: PropTypes.string,
    message: PropTypes.string
}

export default ResultView;
