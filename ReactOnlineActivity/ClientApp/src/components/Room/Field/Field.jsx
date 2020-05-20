import React, {Component} from 'react';
import {Stage, Layer, Line} from 'react-konva';
import styles from './field.module.css';
import {RoomHubEvents} from "../RoomConstants";

export default class Field extends Component {
    constructor(props) {
        super(props);
        this.state = {
            lines: [],
            loadingField: true,
            drawing: false,
            stageRef: null,
            isActiveUser: false,
            activeUser: null,
            hiddenWord: null
        };
    }

    async componentDidMount() {
        this.props.hubConnection.on(RoomHubEvents.NEW_LINE, (line) => {
            this.setState({
                lines: [...this.state.lines, line]
            });
        });

        this.props.hubConnection.on(RoomHubEvents.CLEAR_FIELD, () => {
            this.setState({
                lines: []
            });
        });

        this.props.hubConnection.on(RoomHubEvents.NEW_ROUND, (explainingPlayer) => {
            if (explainingPlayer.name === this.props.user.name) {
                this.setState({ isActiveUser: true });
            }
            
            this.props.hubConnection.invoke(RoomHubEvents.REQUEST_WORD, this.props.roomId);
        });

        this.props.hubConnection.on(RoomHubEvents.NEW_HIDDEN_WORD, (hiddenWord) => {
            this.setState({ hiddenWord })
        });
        
        await this.fetchLines();
    }

    fetchLines = async () => {
        const response = await fetch(`/api/fields/${this.props.roomId}`);
        const lines = await response.json();
        console.log(lines);
        this.setState({
            loadingField: false,
            lines: lines || []
        });
    }

    addLine = () => {
        this.props.hubConnection.invoke(RoomHubEvents.NEW_LINE, 
            this.props.roomId, this.state.lines[this.state.lines.length - 1]);
    };

    handleMouseDown = () => {
        this.state.drawing = true;
        this.setState({
            lines: [...this.state.lines, []]
        });
    };

    handleMouseUp = () => {
        this.state.drawing = false;
        this.addLine();
    };

    handleMouseMove = () => {
        if (!this.state.drawing) {
            return;
        }
        const stage = this.state.stageRef.getStage();
        const point = stage.getPointerPosition();
        const {lines} = this.state;
        let lastLine = lines[lines.length - 1] || [];
        lastLine = lastLine.concat([point.x, point.y]) || [];
        lines.splice(lines.length - 1, 1, lastLine);
        this.setState({
            lines: lines.concat()
        });
    };

    clearField = () => {
        this.setState({lines: []});
        this.props.hubConnection.invoke(RoomHubEvents.CLEAR_FIELD, this.props.roomId);
    }

    render() {
        if (typeof window === 'undefined') {
            return null;
        }

        return (
            <section className={styles.field}>
                {this.state.isActiveUser ?
                    <section>
                        <span
                            className={`btn btn-outline-warning btn-sm ${styles.word}`}
                        >
                            {this.state.hiddenWord}
                        </span>
                        <button
                            className={`btn btn-outline-warning btn-sm ${styles.clear}`}
                            onClick={this.clearField}
                        >
                            Очистить
                        </button>
                        <button
                            className={`btn btn-outline-primary btn-sm ${styles.end}`}
                        >
                            Завершить
                        </button>
                    </section> : ''
                }
                {this.state.loadingField
                    ? <div className={styles.loading}>
                        <p>Загрузка...</p>
                    </div>
                    : <Stage
                        width={window.innerWidth}
                        height={window.innerHeight}
                        onContentMousedown={this.state.isActiveUser ? this.handleMouseDown : () => {}}
                        onContentMousemove={this.state.isActiveUser ? this.handleMouseMove : () => {}}
                        onContentMouseup={this.state.isActiveUser ? this.handleMouseUp : () => {}}
                        ref={node => {this.state.stageRef = node}}>
                        <Layer>
                            {/*this.state.lines.value &&*/this.state.lines.map((line, i) => (
                                <Line key={i} points={line} stroke="blue"/>
                            ))}
                        </Layer>
                    </Stage>
                }
            </section>
        )
    }
}
