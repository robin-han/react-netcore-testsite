import React, { Component } from 'react';
import PropTypes from 'prop-types';
import TreeNode from './TreeNode';

import './tree-view.less';

class TreeView extends Component {
    constructor(props) {
        super(props)

        this.state = {
            activeItem: null
        };
        this.handleLeafNodeClick = this.handleLeafClick.bind(this);
    }

    getChildContext() {
        const { items } = this.props;
        const { activeItem } = this.state;
        return {
            allItems: items,
            activeItem: activeItem,
            onLeafClick: (item) => {
                this.handleLeafClick(item);
            }
        }
    }

    handleLeafClick(item) {
        const { onActiveItemChange } = this.props;
        const { activeItem } = this.state;

        if (activeItem != item) {
            this.setState({ activeItem: item });
            if (onActiveItemChange) {
                onActiveItemChange(item);
            }
        }
    }

    render() {
        const { groups } = this.props;

        return (
            <div className="tree-view">
                {groups.map((g, i) => (
                    <TreeNode key={i} {...g}>
                    </TreeNode>
                ))}
            </div>
        );
    }
}

TreeView.propTypes = {
    items: PropTypes.object.isRequired,
    groups: PropTypes.array.isRequired,
    onActiveItemChange: PropTypes.func
}

TreeView.childContextTypes = {
    allItems: PropTypes.object.isRequired,
    activeItem: PropTypes.object,
    onLeafClick: PropTypes.func
}

export default TreeView;